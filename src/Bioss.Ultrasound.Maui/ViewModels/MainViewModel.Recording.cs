using Acr.UserDialogs;
using Bioss.Ultrasound.Core.Ble.Models;
using Bioss.Ultrasound.Core.Data.Database.Entities.Enums;
using Bioss.Ultrasound.Core.Domain.Constants;
using Bioss.Ultrasound.Core.Domain.Models;
using Bioss.Ultrasound.Core.Resources.Localization;
using Bioss.Ultrasound.Core.Services;
using Bioss.Ultrasound.Maui.Helpers; // поправьте namespace под ваши helpers
using CommunityToolkit.Mvvm.Input;
using Bioss.Ultrasound.Core.Extensions;
using OxyPlot;

namespace Bioss.Ultrasound.Maui.ViewModels;

public partial class MainViewModel
{
    private readonly PlottingTimeSpanHelper _plottingTimeSpanHelper = new PlottingTimeSpanHelper();
    private readonly PlottingHelper _plottingHelper = new PlottingHelper();
    private ChartDrawer _chartDrawer;

    private readonly RecordTimePassedHelper _recordTimePassedHelper = new RecordTimePassedHelper();
    private readonly LossPercentageHelper _lossHelper = new();

    private byte _fhr;
    private byte _toco;
    private int _fetalMovements;
    private byte _batteryLevel = 50;

    private Record _record;

    private bool _isCalculationRunning = false;
    private readonly long _intervalCalculatingTicks = TimeSpan.FromSeconds(5).Ticks;
    private DateTime _lastCalculationDateUtc = DateTime.MinValue;

    private const int RenderFps = 4;
    private volatile bool _hasNewChartData;
    private int _nowRendering;
    private bool _renderLoopRunning;

    // ===== properties (без [ObservableProperty]) =====

    private PlotModel _plotModel;
    public PlotModel PlotModel
    {
        get => _plotModel;
        private set => SetProperty(ref _plotModel, value);
    }

    private bool _isRecording;
    public bool IsRecording
    {
        get => _isRecording;
        set
        {
            if (!SetProperty(ref _isRecording, value))
                return;

            OnPropertyChanged(nameof(IsCloseButtonVisible));
        }
    }

    private string _recordTimePassed;
    public string RecordTimePassed
    {
        get => _recordTimePassed;
        set => SetProperty(ref _recordTimePassed, value);
    }

    private byte _uiFhr;
    public byte FHR
    {
        get => _uiFhr;
        set => SetProperty(ref _uiFhr, value);
    }

    private byte _uiToco;
    public byte Toco
    {
        get => _uiToco;
        set => SetProperty(ref _uiToco, value);
    }

    private int _uiFetalMovements;
    public int FetalMovements
    {
        get => _uiFetalMovements;
        set => SetProperty(ref _uiFetalMovements, value);
    }

    private byte _uiBatteryLevel;
    public byte BatteryLevel
    {
        get => _uiBatteryLevel;
        set
        {
            if (!SetProperty(ref _uiBatteryLevel, value))
                return;

            IsLowBatteryLevel = _batteryLevel <= 25;
        }
    }

    private bool _isLowBatteryLevel;
    public bool IsLowBatteryLevel
    {
        get => _isLowBatteryLevel;
        set
        {
            if (!SetProperty(ref _isLowBatteryLevel, value))
                return;

            if (value && _appSettings.IsBatteryLowSound)
                PlayBell(Sounds.LowBattery, true);
            else
                _audioService.Stop(Sounds.LowBattery);
        }
    }

    private double _soundLevel;
    public double SoundLevel
    {
        get => _soundLevel;
        set => SetProperty(ref _soundLevel, value);
    }

    private bool _isLossData;
    public bool IsLossData
    {
        get => _isLossData;
        set
        {
            if (!SetProperty(ref _isLossData, value))
                return;

            if (value && IsRecording && _appSettings.IsLossDataSound)
                PlayBell(Sounds.LossData, true);
            else
                _audioService.Stop(Sounds.LossData);
        }
    }

    private double _lossPercentage;
    public double LossPercentage
    {
        get => _lossPercentage;
        set => SetProperty(ref _lossPercentage, value);
    }

    private string _lossPercentageMinute;
    public string LossPercentageMinute
    {
        get => _lossPercentageMinute;
        set => SetProperty(ref _lossPercentageMinute, value);
    }

    private bool _isBell;
    public bool IsBell
    {
        get => _isBell;
        set => SetProperty(ref _isBell, value);
    }

    public bool IsCloseButtonVisible => !IsRecording && IsConnected;

    private void InitRecordingPart()
    {
        _device.NewPackage += OnNewPackage;

        _pcmPlayer.Init();
        _pcmPlayer.Start();

        _chartDrawer = new ChartDrawer(_plottingHelper);

        SoundLevel = _systemVolume.Volume;
        PlotModel = _chartDrawer.Model;

        MessagingCenter.Subscribe<object>(this, MessagingCenterConstants.TocoReseting, a =>
        {
            AddTocoReset();
        });

        _systemVolume.VolumeChanged += (a, e) => SoundLevel = e;
    }

    private async Task OnConnectionStateChangedAsync(bool connected)
    {
        if (connected)
        {
            StartRenderLoop();
        }
        else
        {
            ClearChart();

            if (IsRecording)
                await SaveCurrentRecordAsync();
        }
    }

    // ===== commands =====

    [RelayCommand]
    private void Appearing()
    {
        try
        {
            _plottingHelper.Scale = _appSettings.ChartXScaleSeconds;
            _chartDrawer.ResetFhrMinMax(_appSettings.ChartYMinimum, _appSettings.ChartYMaximum);
        }
        catch (Exception ex)
        {
            _logger.Log($"Error when opening main page. Error: {ex}");
        }
    }

    [RelayCommand]
    private async Task RecordAsync()
    {
        if (IsRecording)
        {
            _dialogs.Toast(AppStrings.Main_HoldStopButton);
            return;
        }

        IsRecording = true;
        _isCalculationRunning = false;
        _lastCalculationDateUtc = DateTime.Now;

        _logger.Log($"Started recording with {_device.Name}");

        _record = new Record
        {
            StartTime = DateTime.Now,
            Biometric = new Biometric(),
            DeviceSerialNumber = _device.Name
        };

        ClearChart();

        _recordTimePassedHelper.Init(
            _appSettings.IsAutoRecordTime,
            (int)TimeSpan.FromMinutes(_appSettings.RecordTimeMinutes).TotalSeconds,
            DateTime.Now);

        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task LongPressRecordAsync()
    {
        await SaveCurrentRecordAsync();
    }

    [RelayCommand]
    private async Task ResetTocoAsync()
    {
        Vibro();

        await _device.ResetTocoAsync();
        AddTocoReset();
    }

    [RelayCommand]
    private void ResetFM()
    {
        if (_record == null)
            return;

        Vibro();

        var now = DateTime.Now;
        _record.Events.Add(new FhrEvent { Time = now, Event = Events.FetalMovement });

        _fetalMovements++;
        _chartDrawer.AddFMAnnotation(_plottingTimeSpanHelper.CollectTimeSpan(now));
    }

    [RelayCommand]
    private void BellOff()
    {
        _audioService.Stop();
        IsBell = false;
    }

    private void Vibro()
    {
        try
        {
            HapticFeedback.Perform(HapticFeedbackType.Click);
        }
        catch { }
    }

    // ===== packages =====
    private async void OnNewPackage(object sender, Package package)
    {
        try
        {
            if (await StopRecord(_recordTimePassedHelper.IsTimeEnd, AppStrings.Dialog_RecordCompleted))
            {
                _logger.Log("The timer recording was stopped");
                return;
            }

            if (await StopRecord(_record?.CardiotocographyInfo?.IsRoodDawsonCriteriaValid() ?? false, AppStrings.Dialog_CriteriaMet))
            {
                _logger.Log("The recording was stopped according to the Dawes-Redman criteria");
                return;
            }

            if (package.FHRPackage != null)
            {
                var fhrPackage = package.FHRPackage;

                _fhr = fhrPackage.Fhr1;
                _toco = fhrPackage.Toco;
                _batteryLevel = fhrPackage.Status2.BatteryLevel.GetDigitBatteryLevel();

                UpdateDataPlots(_fhr, _toco);
                _lossHelper.Add(_fhr);
            }

            var decoded = package.SoundPackage.Decompress();
            for (var i = 0; i < decoded.Length; ++i)
                decoded[i] = (short)(decoded[i] - 512);

            _pcmPlayer.AddSound(decoded);
            WriteRecord(package, decoded);

            CalculateCriteria();
        }
        catch (Exception ex)
        {
            _logger.Log($"Error when getting new package: {ex}");
        }
    }

    private void CalculateCriteria()
    {
        try
        {
            if (!IsRecording
                || !_recordTimePassedHelper.IsAutoRecord
                || !_appSettings.IsAutoCompleteRecordByCriteria
                || _record is null
                || _record.Events is null
                || _record.Fhrs is null
                || _record.RecordingTimeSpan.TotalMinutes < CardiograhyConstants.MinRecordingDuration)
                return;

            if (_isCalculationRunning
                && DateTime.UtcNow.Ticks - _lastCalculationDateUtc.Ticks < _intervalCalculatingTicks)
                return;

            _isCalculationRunning = true;

            Task.Run(() =>
            {
                _catAnaService.CargiographAnalayzeWithUserSettings(_record);
                Volatile.Write(ref _isCalculationRunning, false);
                _lastCalculationDateUtc = DateTime.UtcNow;
            });
        }
        catch (Exception ex)
        {
            _isCalculationRunning = false;
            _logger.Log($"Error when getting new package: {ex}");
        }
    }

    private async Task<bool> StopRecord(bool conditionStop, string confirmText)
    {
        if (!IsRecording || !conditionStop)
            return false;

        _recordTimePassedHelper.IsAutoRecord = false;

        if (_appSettings.IsConfirmRecordCompleated)
        {
            PlayBell(Sounds.Attention, true);

            var result = await _dialogs.ConfirmAsync(confirmText, string.Empty, AppStrings.Yes, AppStrings.Continue);

            _audioService.Stop(Sounds.Attention);

            if (!result)
                return false;
        }

        await SaveCurrentRecordAsync();
        return true;
    }

    private async Task SaveCurrentRecordAsync()
    {
        try
        {
            _renderLoopRunning = false;
            IsRecording = false;

            var recordToSave = _record;
            _record = null;

            recordToSave.StopTime = DateTime.Now;

            using (UserDialogs.Instance.Loading(AppStrings.PleaseWait))
            {
                await _repository.InsertAsync(recordToSave);
            }

            var duretionRecord = recordToSave.StopTime - recordToSave.StartTime;
            _logger.Log($"Recording ended on the device{_device.Name}, the recording lasted {duretionRecord}");
        }
        catch (Exception ex)
        {
            _logger.Log($"Error when save record for {_device.Name}: {ex}");
        }
    }

    private void WriteRecord(Package package, short[] soundRaw)
    {
        if (_record is null)
            return;

        foreach (var d in soundRaw)
            _record.Audio.Sound.Add(d);

        if (package.FHRPackage == null)
            return;

        var fhr = package.FHRPackage;

        _record.Fhrs.Add(new FhrData()
        {
            Time = DateTime.Now,
            Fhr = fhr.Fhr1,
            Toco = fhr.Toco
        });
    }

    private void UpdateDataPlots(in byte heartRate, in byte toco)
    {
        var time = _plottingTimeSpanHelper.CollectTimeSpan(DateTime.Now);
        _chartDrawer.Update(time, heartRate, toco);
        _hasNewChartData = true;
    }

    private void ClearChart()
    {
        _chartDrawer.Clear();
        _chartDrawer.InvalidatePlot();

        _plottingTimeSpanHelper.Reset();

        FetalMovements = 0;

        _lossHelper.Clear();
        _toco = 0;
        _fhr = 0;
        _fetalMovements = 0;
        _hasNewChartData = false;

        // как было: сброс без вызова логики setter
        _isLossData = false;
        OnPropertyChanged(nameof(IsLossData));

        _isCalculationRunning = false;
    }

    private void AddTocoReset()
    {
        if (_record == null)
            return;

        var now = DateTime.Now;
        _record.Events.Add(new FhrEvent { Time = now, Event = Events.TocoReset });
        _chartDrawer.AddTocoAnnotation(_plottingTimeSpanHelper.CollectTimeSpan(now));
    }

    private void PlayBell(in Sounds sound, in bool loop = false)
    {
        if (loop)
            IsBell = true;

        _systemVolume.Volume = _appSettings.SoundLevel;
        _audioService.Play(sound, loop);
    }

    private void StartRenderLoop()
    {
        if (_renderLoopRunning)
            return;

        _renderLoopRunning = true;

        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null)
            return;

        dispatcher.StartTimer(TimeSpan.FromMilliseconds(1000 / RenderFps), () =>
        {
            if (!IsConnected)
            {
                _renderLoopRunning = false;
                return false;
            }

            if (Interlocked.Exchange(ref _nowRendering, 1) == 1)
                return true;

            try
            {
                RecordTimePassed = _recordTimePassedHelper.DisplayTimePassed();
                FHR = _fhr;
                Toco = _toco;
                FetalMovements = _fetalMovements;

                BatteryLevel = _batteryLevel;

                LossPercentage = Math.Round(_lossHelper.PercentAll() * 100, 0);
                IsLossData = _lossHelper.IsError && IsRecording;

                var newMinute = _lossHelper.IsQueryFull
                    ? $"{Math.Round(_lossHelper.PercentInMin() * 100, 0)}"
                    : "-";

                if (LossPercentageMinute != newMinute)
                    LossPercentageMinute = newMinute;

                var time = _plottingTimeSpanHelper.CollectTimeSpan(DateTime.Now);
                _plottingHelper.ResetAxisWithMax(time);

                if (_hasNewChartData)
                {
                    _hasNewChartData = false;
                    _chartDrawer.InvalidateGraficPlot();
                }
            }
            finally
            {
                Interlocked.Exchange(ref _nowRendering, 0);
            }

            return true;
        });
    }
}