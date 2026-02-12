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
using Bioss.Ultrasound.Services.Logging;
using Bioss.Ultrasound.Services.Logging.Abstracts;
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
    /// <summary>
    /// 1) OnNewPackage - изолировать от UI потока
    /// 2) оптимизировать через in все синхронные методы
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private static readonly IReadOnlyCollection<string> DevicePrefixesFilter = new string[]
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

        private readonly IUserDialogs _dialogs;
        private readonly DevicesScaner _devicesScaner;
        private readonly IRepository _repository;
        private readonly AppSettingsService _appSettings;
        private readonly IMyDevice _device;
        private readonly IPcmPlayer _pcmPlayer;
        private readonly AudioService _audioService;
        private readonly ISystemVolume _systemVolume;
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
        private bool _renderLoopRunning;

        #region Оптимизация рассчетов критериев
        /// <summary>
        /// Запущен рассчет или нет
        /// Если идет рассчет записи критериев, то другие рассчеты не начинаются, пока параметр не изменит значение
        /// </summary>
        private bool _isCalculationRunning = false;
        /// <summary>
        /// Минимальное время прошедшее между рассчетами
        /// Перерыв нужен, чтобы успели набраться новые данные и выполниться другие фоноые процессы
        /// </summary>
        private readonly long _intervalCalculatingTicks = TimeSpan.FromSeconds(10).Ticks;
        /// <summary>
        /// Время последнего запуска рассчета
        /// </summary>
        private DateTime _lastCalculationDateUtc = DateTime.MinValue;
        #endregion

        public MainViewModel(
            IUserDialogs dialogs,
            DevicesScaner devicesScaner,
            IRepository repository,
            AppSettingsService appSettings,
            IMyDevice device,
            IPcmPlayer pcmPlayer,
            AudioService audioService,
            ISystemVolume systemVolume,
            ILicenseService licenseService,
            CatAnaService catAnaService,
            ILogger logger)
        {
            _dialogs = dialogs;
            _devicesScaner = devicesScaner;
            _repository = repository;
            _appSettings = appSettings;
            _device = device;
            _pcmPlayer = pcmPlayer;
            _audioService = audioService;
            _systemVolume = systemVolume;
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
            _devicesScaner.Start();
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
            try
            {
                _plottingHelper.Scale = _appSettings.ChartXScaleSeconds;
                _chartDrawer.ResetFhrMinMax(_appSettings.ChartYMinimum, _appSettings.ChartYMaximum);
            }
            catch(Exception ex)
            {
                _logger.Log($"Error when opening main page. Error: {ex}", ServerLogLevel.CriticalFunctionalityError);
            }
        });

        /// <summary>
        /// Выбор устройства пользователем для подключения
        /// </summary>
        public ICommand SelectedDeviceCommand => new AsyncCommand(async () =>
        {
            if (SelectedDevice is null)
                return;

            try
            {
                var isLicense = await _licenseService.CheckDeviceLicenseAsync(SelectedDevice.Name);

                var selectedDevice = SelectedDevice;
                SelectedDevice = null;
                if (isLicense)
                    await _device.ConnectAsync(selectedDevice);
                else
                {
                    _dialogs.Toast(new ToastConfig(AppStrings.Main_DeviceNotLicense)
                    {
                        Position = ToastPosition.Top,
                        BackgroundColor = Color.DeepSkyBlue,
                        MessageTextColor = Color.White
                    });
                }
            }
            catch(Exception ex)
            {
                _logger.Log($"Error when selectively connecting to the device({_device.Name}). Error: {ex}", ServerLogLevel.CriticalFunctionalityError);
            }

        }, allowsMultipleExecutions: false);

        /// <summary>
        /// Отключение от устройства
        /// </summary>
        public ICommand DisconnectCommand => new AsyncCommand(async () =>
        {
            if (!await _dialogs.ConfirmAsync(AppStrings.Dialog_DisconnectMessage, string.Empty, AppStrings.Yes, AppStrings.Cancel))
                return;

            try
            {
                await _device.DisconnectAsync();
                _logger.Log($"Disconnected the device {_device.Name}");
            }
            catch (Exception ex)
            {
                _logger.Log($"Errors when trying to disconnect the device({_device.Name}). Error: {ex}", ServerLogLevel.CriticalFunctionalityError);
            }
            
            //  На всякий случай останавливаем звуковой сигнал, если он вдруг включен
            BatteryLevel = 100;
            IsBell = false;
        });

        /// <summary>
        /// Команда начала записи.
        /// Если записиь начата, то эта же кнопка будет останавливать запись, если держать достаочно долго
        /// </summary>
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
            _lossHelper.Clear();
            StartRenderLoop();
            _recordTimePassedHelper.Init(
                _appSettings.IsAutoRecordTime,
                (int)TimeSpan.FromMinutes(_appSettings.RecordTimeMinutes).TotalSeconds,
                DateTime.Now);
        },
        allowsMultipleExecutions: false);

        /// <summary>
        /// Команда остановки записи
        /// </summary>
        public ICommand LongPressRecordCommand => new AsyncCommand(async () =>
        {
            await SaveCurrentRecordAsync();
        }, allowsMultipleExecutions: false);

        /// <summary>
        /// Сброс показателей TOCO
        /// </summary>
        public ICommand ResetTocoCommand => new Command(async a =>
        {
            Vibro();

            await _device.ResetTocoAsync();
            AddTocoReset();
        });

        /// <summary>
        /// Зафискировано движение плода
        /// </summary>
        public ICommand ResetFMCommand => new Command(a =>
        {
            if (_record == null)
                return;

            Vibro();

            var now = DateTime.Now;
            _record.Events.Add(new FhrEvent { Time = now, Event = Events.FetalMovement });

            _fetalMovements++;
            _chartDrawer.AddFMAnnotation(_plottingTimeSpanHelper.CollectTimeSpan(now));
        });

        /// <summary>
        /// Команда для ввода данных о пациенте
        /// </summary>
        public ICommand BiometricCommand => new AsyncCommand(async () =>
        {
            var popup = new BiometricPopup(_dialogs, _record.Biometric);
            await PopupNavigation.Instance.PushAsync(popup);
            var result = await popup.PopupClosedTask;

        }, allowsMultipleExecutions: false);

        /// <summary>
        /// Остановка всех звонков
        /// </summary>
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
        /// <summary>
        /// Изменение состояния подключения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="isConnected">сосотяние подключения</param>
        private async void OnConnectedChanged(object sender, bool isConnected)
        {
            IsConnected = isConnected;
            Devices.Clear();

            if (isConnected)
                await _devicesScaner.StopAsync();
            else
                _devicesScaner.Start();



            if (IsRecording && !isConnected)
            {
                //  TODO: нужно сделать алгоритм реконнекта

                //  TODO: пока что просто сохраним то что намеряли до этого
                await SaveCurrentRecordAsync();
            }
        }

        
        /// <summary>
        /// Обработка полученного пакета, перевод сигналов датчика в запись
        /// </summary>
        /// <param name="sender">устройство</param>
        /// <param name="package">пакет с данными датчиками</param>
        private async void OnNewPackage(object sender, Package package)
        {
            try
            {
                if (await StopRecord(_recordTimePassedHelper.IsTimeEnd, AppStrings.Dialog_RecordCompleted))
                {
                    _logger.Log("The timer recording was stopped");
                    return;
                }

                if (package.FHRPackage != null)
                {
                    var fhrPackage = package.FHRPackage;

                    _fhr = fhrPackage.Fhr1;
                    _toco = fhrPackage.Toco;

                    switch (fhrPackage.Status2.BatteryLevel)
                    {
                        case Ble.Models.Enums.BatteryLevel.Excellent:
                            _batteryLevel = 100;
                            break;
                        case Ble.Models.Enums.BatteryLevel.Good:
                            _batteryLevel = 75;
                            break;
                        case Ble.Models.Enums.BatteryLevel.Normal:
                            _batteryLevel = 50;
                            break;
                        case Ble.Models.Enums.BatteryLevel.Bad:
                            _batteryLevel = 25;
                            break;
                        case Ble.Models.Enums.BatteryLevel.Critical:
                            _batteryLevel = 0;
                            break;
                    }

                    UpdateDataPlots(_fhr, _toco);
                    _lossHelper.Add(_fhr);
                }

                var decoded = package.SoundPackage.Decompress();

                //  опускаем сигнал вниз, так как при отсутствии значений, он равен 512
                for (var i = 0; i < decoded.Length; ++i)
                    decoded[i] = (short)(decoded[i] - 512);

                _pcmPlayer.AddSound(decoded);
                WriteRecord(package, decoded);

                await CalculateCriteriaAsync();
            }
            catch(Exception ex)
            {
                _logger.Log($"Error when getting new package: {ex}", ServerLogLevel.Warn);
            }
        }

        /// <summary>
        /// Вычисления связанные с Кртиериями Доуса Редмана
        /// </summary>
        /// <returns></returns>
        private async ValueTask CalculateCriteriaAsync()
        {
            try
            {
                // Обязательные условия для начала рассчетов
                if (!IsRecording
                        || !_recordTimePassedHelper.IsAutoRecord
                        || !_appSettings.IsAutoCompleteRecordByCriteria
                        || _record is null
                        || _record.Events is null
                        || _record.Fhrs is null
                        || _record.RecordingTimeSpan.TotalMinutes < CardiograhyConstants.MinRecordingDuration)
                    return;

                // условия для запуска следующего рассчета
                if (_isCalculationRunning 
                    && DateTime.UtcNow.Ticks - _lastCalculationDateUtc.Ticks < _intervalCalculatingTicks)
                    return;

                _isCalculationRunning = true;

                var cardiografy = _catAnaService.CargiographAnalayzeWithUserSettings(_record);
                if (await StopRecord(cardiografy.IsRoodDawsonCriteriaValid(), AppStrings.Dialog_CriteriaMet))
                    _logger.Log("The recording was stopped according to the Dawes-Redman criteria");
                
                _isCalculationRunning = false;
                _lastCalculationDateUtc = DateTime.UtcNow;
            }
            catch(Exception ex)
            {
                _isCalculationRunning = false;
                _logger.Log($"Error when getting new package: {ex}", ServerLogLevel.Warn);
            }
        }

        /// <summary>
        /// Действия при обнаружении устройств поблизоватси
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="device">найденное устройство</param>
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
                try
                {
                    if (await _licenseService.CheckDeviceLicenseAsync(device.Name))
                        await _device.ConnectAsync(device);
                }
                catch (Exception ex)
                {
                    _logger.Log($"Error when auto-connecting to the device({_device.Name}). Error: {ex}", ServerLogLevel.CriticalFunctionalityError);
                }
            }       
        }
        #endregion

        /// <summary>
        /// Действия по остановке записи
        /// </summary>
        /// <param name="conditionStop">условие остановки</param>
        /// <param name="confirmText">текст для подтверждения остановки</param>
        /// <returns>остановлена ли запись</returns>
        private async Task<bool> StopRecord(bool conditionStop, string confirmText)
        {
            if (!IsRecording || !conditionStop)
                return false;
            

            _recordTimePassedHelper.IsAutoRecord = false;
            if (_appSettings.IsConfirmRecordCompleated)
            {
                PlayBell(Sounds.Attention, true);

                var result = await _dialogs.ConfirmAsync(confirmText, null, AppStrings.Yes, AppStrings.Continue);

                _audioService.Stop(Sounds.Attention);

                if (!result)
                    return false;
            }

            // Реальная остановка находится внутри метода SaveCurrentRecordAsync
            await SaveCurrentRecordAsync();
            return true;
        }

        /// <summary>
        /// Сохранение записи и реальная остановка записи находится здесь
        /// </summary>
        /// <returns></returns>
        private async Task SaveCurrentRecordAsync()
        {
            try
            {
                _renderLoopRunning = false;
                IsRecording = false;
                var recordToSave = _record;
                _record = null;

                recordToSave.StopTime = DateTime.Now;
                using (var loading = UserDialogs.Instance.Loading(AppStrings.PleaseWait))
                {
                    await _repository.InsertAsync(recordToSave);
                }

                var duretionRecord = recordToSave.StopTime - recordToSave.StartTime;
                _logger.Log($"Recording ended on the device{_device.Name}, the recording lasted {duretionRecord}");
            }
            catch (Exception ex)
            {
                _logger.Log($"Error when save record for {_device.Name}: {ex}", ServerLogLevel.CriticalFunctionalityError);
            }
        }

        /// <summary>
        /// Записывае данные пакета в запись, так же начинаем озвучивать звуки измерений
        /// </summary>
        /// <param name="package"></param>
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

        /// <summary>
        /// Обновляем график записи
        /// </summary>
        /// <param name="heartRate">значение ЧСС</param>
        /// <param name="toco">значение TOCO - маточных сокращений</param>
        private void UpdateDataPlots(in byte heartRate, in byte toco)
        {
            var time = _plottingTimeSpanHelper.CollectTimeSpan(DateTime.Now);
            _chartDrawer.Update(time, heartRate, toco);
            _hasNewChartData = true;
        }

        /// <summary>
        /// Очистка графика от записи
        /// </summary>
        private void ClearChart()
        {
            _chartDrawer.Clear();
            _chartDrawer.InvalidatePlot();

            _plottingTimeSpanHelper.Reset();

            FetalMovements = 0;
        }

        /// <summary>
        /// Добавляем событие маточного сокращения
        /// </summary>
        private void AddTocoReset()
        {
            if (_record == null)
                return;

            var now = DateTime.Now;
            _record.Events.Add(new FhrEvent { Time = now, Event = Events.TocoReset });
            _chartDrawer.AddTocoAnnotation(_plottingTimeSpanHelper.CollectTimeSpan(now));
        }

        /// <summary>
        /// Включаем звук оповещения
        /// </summary>
        /// <param name="sound">тип звука</param>
        /// <param name="loop">единично или звенеть пока не отключат</param>
        private void PlayBell(Sounds sound, in bool loop = false)
        {
            if (loop)
                IsBell = true;

            _systemVolume.Volume = _appSettings.SoundLevel;
            _audioService.Play(sound, loop);
        }

        private const int RenderFps = 4;
        private volatile bool _hasNewChartData;
        private void StartRenderLoop()
        {
            if (_renderLoopRunning)
                return;

            _renderLoopRunning = true;

            Device.StartTimer(TimeSpan.FromMilliseconds(1000 / RenderFps), () =>
            {
                if (!_isRecording)
                {
                    _renderLoopRunning = false;
                    return false;
                }

                // обновление свойств
                RecordTimePassed = _recordTimePassedHelper.DisplayTimePassed();
                FHR = _fhr;
                Toco = _toco;
                FetalMovements = _fetalMovements;

                BatteryLevel = _batteryLevel;
                
                LossPercentage = Math.Round(_lossHelper.PercentAll() * 100, 0);
                IsLossData = _lossHelper.IsError && IsRecording && _recordTimePassedHelper.CurrentRecordTime.TotalSeconds > 5; // Чтобы не пересчитывать пока датчик  

                var newMinute = _lossHelper.IsQueryFull
                    ? $"{Math.Round(_lossHelper.PercentInMin() * 100, 0)}"
                    : "-";

                if (LossPercentageMinute != newMinute)
                    LossPercentageMinute = newMinute;

                // обновление графика
                var time = _plottingTimeSpanHelper.CollectTimeSpan(DateTime.Now);
                _plottingHelper.ResetAxisWithMax(time);
                if (_hasNewChartData)
                {
                    _hasNewChartData = false;
                    _chartDrawer.InvalidateGraficPlot();
                }

                return true;
            });
        }
    }
}
