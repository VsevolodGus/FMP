using Acr.UserDialogs;
using Bioss.Ultrasound.Ble;
using Bioss.Ultrasound.Ble.Devices;
using Bioss.Ultrasound.Ble.Models;
using Bioss.Ultrasound.Data.Database.Entities.Enums;
using Bioss.Ultrasound.DependencyExtensions;
using Bioss.Ultrasound.Domain.Constants;
using Bioss.Ultrasound.Domain.Models;
using Bioss.Ultrasound.Domain.Plotting;
using Bioss.Ultrasound.Repository.Abstracts;
using Bioss.Ultrasound.Resources.Localization;
using Bioss.Ultrasound.Services;
using Bioss.Ultrasound.Services.Licenses;
using Bioss.Ultrasound.Services.Logging.Abstracts;
using Bioss.Ultrasound.Tools;
using Bioss.Ultrasound.UI.Helpers;
using Bioss.Ultrasound.UI.Popups;
using Libs.DI.ViewModels;
using OxyPlot;
using Plugin.BLE.Abstractions.Contracts;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Bioss.Ultrasound.UI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private static readonly IReadOnlyCollection<string> DevicePrefixesFilter = new List<string> 
        { 
            "LCeFM",
            "Doctis CTG",
            "FMP", 
            "DOCTIS-CTG" 
        };

        private readonly PlottingTimeSpanHelper _plottingTimeSpanHelper = new PlottingTimeSpanHelper();
        private readonly PlottingHelper _plottingHelper = new PlottingHelper();
        private readonly ChartDrawer _chartDrawer;
        private readonly RecordTimePassedHelper _recordTimePassedHelper = new RecordTimePassedHelper();
        private readonly LossPercentageHelper _lossHelper = new();

        private readonly INavigation _navigation;
        private readonly IUserDialogs _dialogs;
        private readonly DevicesScaner _devicesScaner;
        private readonly IRepository _repository;
        private readonly AppSettingsService _appSettings;
        private readonly IMyDevice _device;
        private readonly IPcmPlayer _pcmPlayer;
        private readonly AudioService _audioService;
        private readonly ISystemVolume _systemVolume;
        private readonly InfoSettingsService _infoSettingsService;
        private readonly ILicenseService _licenseService;
        private readonly CatAnaService _catAnaService;
        private readonly ILogger _logger;



        private IDevice _selectedDevice;


        private bool _isConnected;
        private byte _fhr;
        private byte _toco;
        private int _fetalMovements;
        private byte _batteryLevel;

        private PlotModel _plotModel;

        private bool _isRecording;
        private Record _record;
        private string _recordTimePassed;
        private bool _isLowBatteryLevel;
        private double _soundLevel;
        private double _lossPercentage;
        private string _lossPercentageMinute;
        private bool _isLossData;

        private bool _isBell;

        public MainViewModel(INavigation navigation, 
            IUserDialogs dialogs, 
            DevicesScaner devicesScaner, 
            IRepository repository,
            AppSettingsService appSettings, 
            IMyDevice device, 
            IPcmPlayer pcmPlayer, 
            AudioService audioService, 
            ISystemVolume systemVolume,
            InfoSettingsService infoSettingsService,
            ILicenseService licenseService,
            CatAnaService catAnaService,
            ILogger logger)
        {
            _navigation = navigation;
            _dialogs = dialogs;
            _devicesScaner = devicesScaner;
            _repository = repository;
            _appSettings = appSettings;
            _device = device;
            _pcmPlayer = pcmPlayer;
            _audioService = audioService;
            _systemVolume = systemVolume;
            _infoSettingsService = infoSettingsService;
            _catAnaService = catAnaService;
            _licenseService = licenseService;
            _logger = logger;

            _devicesScaner.Discovered += OnDeviceDiscovered;
            _device.ConnectedChanged += OnConnectedChanged;
            _device.NewPackage += OnNewPackage;

            _pcmPlayer.Init();
            _pcmPlayer.Start();

            _chartDrawer = new ChartDrawer(_plottingHelper);

            SoundLevel = _systemVolume.Volume;

            IsConnected = _device.IsConnected;

            PlotModel = _chartDrawer.Model;

            MessagingCenter.Subscribe<object>(this, MessagingCenterConstants.TocoReseting, a =>
            {
                AddTocoReset();
            });

            _systemVolume.VolumeChanged += (a, e) => SoundLevel = e;
        }

        #region Поля для UI
        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                SetProperty(ref _isConnected, value);
                OnPropertyChanged(nameof(IsCloseButtonVisible));
            }
        }

        public byte FHR
        {
            get => _fhr;
            set => SetProperty(ref _fhr, value);
        }

        public byte Toco
        {
            get => _toco;
            set => SetProperty(ref _toco, value);
        }

        public int FetalMovements
        {
            get => _fetalMovements;
            set => SetProperty(ref _fetalMovements, value);
        }

        public byte BatteryLevel
        {
            get => _batteryLevel;
            set
            {
                SetProperty(ref _batteryLevel, value);
                IsLowBatteryLevel = _batteryLevel <= 25;
            }
        }

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

        public PlotModel PlotModel
        {
            get => _plotModel;
            set => SetProperty(ref _plotModel, value);
        }

        public bool IsRecording
        {
            get => _isRecording;
            set
            {
                SetProperty(ref _isRecording, value);
                OnPropertyChanged(nameof(IsCloseButtonVisible));
            }
        }

        public string RecordTimePassed
        {
            get => _recordTimePassed;
            set => SetProperty(ref _recordTimePassed, value);
        }

        public bool IsCloseButtonVisible => !IsRecording && IsConnected;

        public ObservableCollection<IDevice> Devices { get; } = new ObservableCollection<IDevice>();

        public IDevice SelectedDevice
        {
            get => _selectedDevice;
            set => SetProperty(ref _selectedDevice, value);
        }

        public double SoundLevel
        {
            get => _soundLevel;
            set => SetProperty(ref _soundLevel, value);
        }

        public double LossPercentage
        {
            get => _lossPercentage;
            set => SetProperty(ref _lossPercentage, value);
        }

        public string LossPercentageMinute
        {
            get => _lossPercentageMinute;
            set => SetProperty(ref _lossPercentageMinute, value);
        }

        public bool IsBell
        {
            get => _isBell;
            set => SetProperty(ref _isBell, value);
        }
        #endregion

        #region ICommand
        public ICommand AppearingCommand => new Command(a =>
        {
            _plottingHelper.Scale = _appSettings.ChartXScaleSeconds;
            _chartDrawer.ResetFhrMinMax(_appSettings.ChartYMinimum, _appSettings.ChartYMaximum);
        });

        public ICommand SelectedDeviceCommand => new AsyncCommand(async () =>
        {
            if (SelectedDevice is null)
                return;

            //await _devicesScaner.StopAsync();

            
            await _device.ConnectAsync(SelectedDevice);
            await _licenseService.CheckDeviceLicenseAsync(SelectedDevice.Name);

            _logger.Log($"Подключили устройство {SelectedDevice.Name}");
            SelectedDevice = null;
        }, allowsMultipleExecutions: false);

        public ICommand DisconnectCommand => new AsyncCommand(async () =>
        {
            if (!await _dialogs.ConfirmAsync(AppStrings.Dialog_DisconnectMessage, string.Empty, AppStrings.Yes, AppStrings.Cancel))
                return;

            _logger.Log($"Отключили устройство {_device.Name}");
            await _device.DisconnectAsync();

            //  На всякий случай останавливаем звуковой сигнал, если он вдруг включен
            BatteryLevel = 100;
            IsBell = false;
        });

        public ICommand RecordCommand => new AsyncCommand(async () =>
        {
            if (IsRecording)
            {
                _dialogs.Toast(new ToastConfig(AppStrings.Main_HoldStopButton)
                {
                    Position = ToastPosition.Top,
                    BackgroundColor = Color.DeepSkyBlue,
                    MessageTextColor = Color.White
                });
                return;
            }

            IsRecording = true;

            _logger.Log($"Начали запись с устройством {_device.Name}");
            _record = new Record
            {
                StartTime = DateTime.Now,
                Biometric = new Biometric(),
                DeviceSerialNumber = _device.Name
            };

            ClearChart();
            _lossHelper.Clear();

            _recordTimePassedHelper.Init(
                _appSettings.IsAutoRecordTime,
                (int)TimeSpan.FromMinutes(_appSettings.RecordTimeMinutes).TotalSeconds,
                DateTime.Now);
        },
        allowsMultipleExecutions: false);

        public ICommand LongPressRecordCommand => new  AsyncCommand(async () =>
        {
            await SaveCurrentRecordAsync();
        }, allowsMultipleExecutions: false);

        public ICommand ResetTocoCommand => new Command(async a =>
        {
            Vibro();

            await _device.ResetTocoAsync();
            AddTocoReset();
        });

        public ICommand ResetFMCommand => new Command(a =>
        {
            if (_record == null)
                return;

            Vibro();

            var now = DateTime.Now;
            _record.Events.Add(new FhrEvent { Time = now, Event = Events.FetalMovement });

            FetalMovements++;
            _chartDrawer.AddFMAnnotation(_plottingTimeSpanHelper.CollectTimeSpan(now));
        });

        public ICommand BiometricCommand => new AsyncCommand(async () =>
        {
            var popup = new BiometricPopup(_dialogs, _record.Biometric);
            await PopupNavigation.Instance.PushAsync(popup);
            var result = await popup.PopupClosedTask;

        }, allowsMultipleExecutions: false);

        public ICommand BellOffCommand => new Command(a =>
        {
            _audioService.Stop();
            IsBell = false;
        });

        private void Vibro()
        {
            try
            {
                HapticFeedback.Perform(HapticFeedbackType.Click);
            }
            catch { }
        }
        #endregion

        #region Events with Bluetooth
        private async void OnConnectedChanged(object sender, bool isConnected)
        {
            IsConnected = isConnected;
            Devices.Clear();

            if (IsRecording && !isConnected)
            {
                //  TODO: нужно сделать алгоритм реконнекта

                //  TODO: пока что просто сохраним то что намеряли до этого
                await SaveCurrentRecordAsync();
            }
        }

        private async void OnNewPackage(object sender, Package package)
        {
            RecordTimePassed = _recordTimePassedHelper.DisplayTimePassed();

            await StopRecord(_recordTimePassedHelper.IsTimeEnd, AppStrings.Dialog_RecordCompleted);
            if (package.FHRPackage != null)
            {
                var fhrPackage = package.FHRPackage;

                FHR = fhrPackage.Fhr1;
                Toco = fhrPackage.Toco;

                switch (fhrPackage.Status2.BatteryLevel)
                {
                    case Ble.Models.Enums.BatteryLevel.Excellent:
                        BatteryLevel = 100;
                        break;
                    case Ble.Models.Enums.BatteryLevel.Good:
                        BatteryLevel = 75;
                        break;
                    case Ble.Models.Enums.BatteryLevel.Normal:
                        BatteryLevel = 50;
                        break;
                    case Ble.Models.Enums.BatteryLevel.Bad:
                        BatteryLevel = 25;
                        break;
                    case Ble.Models.Enums.BatteryLevel.Critical:
                        BatteryLevel = 0;
                        break;
                }

                UpdatePlots(FHR, Toco);

                _lossHelper.Add(FHR);

                LossPercentageMinute = _lossHelper.IsQueryFull
                    ? $"{Math.Round(_lossHelper.PercentInMin() * 100, 0)}"
                    : "-";
                LossPercentage = Math.Round(_lossHelper.PercentAll() * 100, 0);

                IsLossData = _lossHelper.IsError && IsRecording;
            }

            var sound = package.SoundPackage;
            var decoded = sound.Decompress();

            //  опускаем сигнал вниз, так как при отсутствии значений, он равен 512
            for (var i = 0; i < decoded.Length; ++i)
                decoded[i] = (short)(decoded[i] - 512);

            _pcmPlayer.AddSound(decoded);
            
            //
            WriteRecord(package);

            if (_record is null 
                || !_recordTimePassedHelper.IsAutoRecord
                || !_appSettings.IsAutoCompleteRecordByCriteria)
                return;

            var week = _infoSettingsService.PregnancyStart.HasValue
                ? _infoSettingsService.PregnancyStart.Value.CalculatePregnantTime().weeks
                : Constants.DefaultCountWeek;

            var floatRate = _record.Fhrs.Select(c => (float)c.Fhr).ToArray();
            var movement = _record.Events.Select(c => c.Event == Events.FetalMovement).ToArray();
            var cardiografy = _catAnaService.CargiographAnalayzeWithUserSettings(week, floatRate, movement);

            await StopRecord(cardiografy.IsRoodDawsonCriteriaValid(), AppStrings.Dialog_CriteriaMet);
        }

        private async void OnDeviceDiscovered(object sender, IDevice device)
        {
            if (_device is not null && _device.IsConnected)
                return;

            if (device.Name == null || !DevicePrefixesFilter.Any(s => device.Name.StartsWith(s, StringComparison.CurrentCultureIgnoreCase)))
                return;

            if (Devices.Any(a => a.Name == device.Name))
                return;

            Devices.Add(device);

            if (_appSettings.IsAutoConnect && !_device.IsConnected)
            {
                await _device.ConnectAsync(device);
                await _licenseService.CheckDeviceLicenseAsync(device.Name);
            }
                
        }
        #endregion

        private async Task StopRecord(bool conditionStop, string confirmText)
        {
            if (!conditionStop)
                return;
            

            _recordTimePassedHelper.IsAutoRecord = false;
            if (_appSettings.IsConfirmRecordCompleated)
            {
                PlayBell(Sounds.Attention, true);

                var result = await _dialogs.ConfirmAsync(confirmText, null, AppStrings.Yes, AppStrings.Continue);

                _audioService.Stop(Sounds.Attention);

                if (!result)
                    return;
            }

            // Реальная остановка находится внутри метода SaveCurrentRecordAsync
            await SaveCurrentRecordAsync();
        }

        private async Task SaveCurrentRecordAsync()
        {
            IsRecording = false;
            var recordToSave = _record;
            _record = null;

            recordToSave.StopTime = DateTime.Now;
            using (var loading = UserDialogs.Instance.Loading(AppStrings.PleaseWait))
            {
                await _repository.InsertAsync(recordToSave);
            }

            var duretionRecord = recordToSave.StopTime - recordToSave.StartTime;
            _logger.Log($"Законсилась запись {_device.Name}, запись длилась {duretionRecord}");
        }

        private void WriteRecord(Package package)
        {
            if (_record is null)
                return;

            var soundRaw = package.SoundPackage.Decompress();
            foreach (var d in soundRaw)
                _record.Audio.Sound.Add(d);

            if (package.FHRPackage == null)
                return;

            var fhr = package.FHRPackage;
            _record.Fhrs.Add(new FhrData() { Time = DateTime.Now, Fhr = fhr.Fhr1, Toco = fhr.Toco });
        }

        private void UpdatePlots(byte heartRate, byte toco)
        {
            var time = _plottingTimeSpanHelper.CollectTimeSpan(DateTime.Now);

            _chartDrawer.Update(time, heartRate, toco);
            _plottingHelper.ResetAxisWithMax(time);
            _chartDrawer.InvalidatePlot(true);
        }

        private void ClearChart()
        {
            _chartDrawer.Clear();
            _chartDrawer.InvalidatePlot();

            _plottingTimeSpanHelper.Reset();

            FetalMovements = 0;
        }

        private void AddTocoReset()
        {
            if (_record == null)
                return;

            var now = DateTime.Now;
            _record.Events.Add(new FhrEvent { Time = now, Event = Events.TocoReset });
            _chartDrawer.AddTocoAnnotation(_plottingTimeSpanHelper.CollectTimeSpan(now));
        }

        private void PlayBell(Sounds sound, bool loop = false)
        {
            if (loop)
                IsBell = true;

            _systemVolume.Volume = _appSettings.SoundLevel;
            _audioService.Play(sound, loop);
        }
    }
}
