using System.Collections.ObjectModel;
using Bioss.Ultrasound.Core;
using Bioss.Ultrasound.Core.DependencyExtensions;
using Bioss.Ultrasound.Core.Resources.Localization;
using Bioss.Ultrasound.Core.Services;
using Bioss.Ultrasound.Core.Tools;
using Bioss.Ultrasound.Domain.UI;
using Bioss.Ultrasound.Maui.Navigation;
using Bioss.Ultrasound.Maui.Pages;
using Bioss.Ultrasound.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Bioss.Ultrasound.Maui.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly AppSettingsService _appSettings;
    private readonly InfoSettingsService _infoSettingsService;
    private readonly AutoResetTocoService _autoResetTocoService;
    private readonly ISystemVolume _systemVolume;
    private readonly AudioService _audioService;
    private readonly INavigationService _navigation;

    private PickerItem<int> _autoRecordTime;
    private PickerItem<int> _chartXScale;
    private PickerItem<(int, int)> _chartYScale;
    private PickerItem<int> _recordingSpeed;

    public SettingsViewModel(AppSettingsService appSettings,
        InfoSettingsService infoSettingsService,
        AutoResetTocoService autoResetTocoService,
        ISystemVolume systemVolume,
        AudioService audioService,
        INavigationService navigation
        )
    {
        _appSettings = appSettings;
        _infoSettingsService = infoSettingsService;
        _autoResetTocoService = autoResetTocoService;
        _systemVolume = systemVolume;
        _audioService = audioService;
        _navigation = navigation;

        AutoRecordTime = AutoRecordTimes.FirstOrDefault(a => a.Value == _appSettings.RecordTimeMinutes);
        ChartXScale = ChartXScales.FirstOrDefault(a => a.Value == _appSettings.ChartXScaleSeconds);
        ChartYScale = ChartYScales.FirstOrDefault(a => a.Value.Item1 == _appSettings.ChartYMinimum && a.Value.Item2 == _appSettings.ChartYMaximum);

        RecordingSpeed = RecordingSpeeds.FirstOrDefault(a => a.Value == _infoSettingsService.PdfRecordingSpeed);
    }

    [RelayCommand]
    private async Task PrivacyCommand()
       => await _navigation.NavigateToAsync<DocumentPage>();

    public bool IsAutoToco
    {
        get => _appSettings.IsAutoResetToco;

        set
        {
            _appSettings.IsAutoResetToco = value;
            OnPropertyChanged();

            _autoResetTocoService.IsAutoResetToco = _appSettings.IsAutoResetToco;
        }
    }

    public bool IsAutoConnect
    {
        get => _appSettings.IsAutoConnect;
        set => _appSettings.IsAutoConnect = value;
    }

    public bool IsAutoRecordTime
    {
        get => _appSettings.IsAutoRecordTime;
        set
        {
            _appSettings.IsAutoRecordTime = value;
            OnPropertyChanged();
        }
    }

    public bool ConfirmRecordCompleated
    {
        get => _appSettings.IsConfirmRecordCompleated;
        set => _appSettings.IsConfirmRecordCompleated = value;
    }

    public ObservableCollection<PickerItem<int>> AutoRecordTimes { get; } = new ObservableCollection<PickerItem<int>>
    {
        new PickerItem<int> { Name = string.Format(AppStrings.Settings_MonitorFinishMinuteFormat, 10), Value = 10 },
        new PickerItem<int> { Name = string.Format(AppStrings.Settings_MonitorFinishMinuteFormat, 20), Value = 20 },
        new PickerItem<int> { Name = string.Format(AppStrings.Settings_MonitorFinishMinuteFormat, 30), Value = 30 },
        new PickerItem<int> { Name = string.Format(AppStrings.Settings_MonitorFinishMinuteFormat, 40), Value = 40 },
        new PickerItem<int> { Name = string.Format(AppStrings.Settings_MonitorFinishMinuteFormat, 50), Value = 50 },
        new PickerItem<int> { Name = string.Format(AppStrings.Settings_MonitorFinishMinuteFormat, 60), Value = 60 },
    };

    public PickerItem<int> AutoRecordTime
    {
        get => _autoRecordTime;
        set
        {
            _autoRecordTime = value;
            OnPropertyChanged();

            if (_autoRecordTime == null)
                return;

            _appSettings.RecordTimeMinutes = value.Value;
        }
    }

    public bool IsAutoCompleteRecordByCriteria
    {
        get => _appSettings.IsAutoCompleteRecordByCriteria;
        set => _appSettings.IsAutoCompleteRecordByCriteria = value;
    }


    public ObservableCollection<PickerItem<int>> ChartXScales { get; } = new ObservableCollection<PickerItem<int>>
    {
        new PickerItem<int> { Name = string.Format(AppStrings.Settings_ChartVariantsMinuteFormat, 1), Value = 1 * 60 },
        new PickerItem<int> { Name = string.Format(AppStrings.Settings_ChartVariantsMinuteFormat, 2), Value = 2 * 60 },
        new PickerItem<int> { Name = string.Format(AppStrings.Settings_ChartVariantsMinuteFormat, 3), Value = 3 * 60 },
        new PickerItem<int> { Name = string.Format(AppStrings.Settings_ChartVariantsMinuteFormat, 4), Value = 4 * 60 },
        new PickerItem<int> { Name = string.Format(AppStrings.Settings_ChartVariantsMinuteFormat, 5), Value = 5 * 60 },
        new PickerItem<int> { Name = string.Format(AppStrings.Settings_ChartVariantsMinuteFormat, 6), Value = 6 * 60 },
        new PickerItem<int> { Name = string.Format(AppStrings.Settings_ChartVariantsMinuteFormat, 7), Value = 7 * 60 },
        new PickerItem<int> { Name = string.Format(AppStrings.Settings_ChartVariantsMinuteFormat, 8), Value = 8 * 60 },
        new PickerItem<int> { Name = string.Format(AppStrings.Settings_ChartVariantsMinuteFormat, 9), Value = 9 * 60 },
        new PickerItem<int> { Name = string.Format(AppStrings.Settings_ChartVariantsMinuteFormat, 10), Value = 10 * 60 },
    };

    public PickerItem<int> ChartXScale
    {
        get => _chartXScale;
        set
        {
            _chartXScale = value;
            OnPropertyChanged();

            if (_chartXScale == null)
                return;

            _appSettings.ChartXScaleSeconds = value.Value;
        }
    }

    public ObservableCollection<PickerItem<(int, int)>> ChartYScales { get; } = new ObservableCollection<PickerItem<(int, int)>>
    {
        new PickerItem<(int, int)> { Name = string.Format(AppStrings.Settings_ChartYScaleFormat, 30, 240), Value = (30, 240) },
        new PickerItem<(int, int)> { Name = string.Format(AppStrings.Settings_ChartYScaleFormat, 50, 220), Value = (50, 220) },
    };

    public PickerItem<(int, int)> ChartYScale
    {
        get => _chartYScale;
        set
        {
            _chartYScale = value;
            OnPropertyChanged();

            if (_chartYScale == null)
                return;

            _appSettings.ChartYMinimum = value.Value.Item1;
            _appSettings.ChartYMaximum = value.Value.Item2;
        }
    }

    public bool IsBatteryLowSound
    {
        get => _appSettings.IsBatteryLowSound;
        set => _appSettings.IsBatteryLowSound = value;
    }

    public bool IsLossDataSound
    {
        get => _appSettings.IsLossDataSound;
        set => _appSettings.IsLossDataSound = value;
    }

    // Info

    public string Doctor
    {
        get => _infoSettingsService.Doctor;
        set => _infoSettingsService.Doctor = value;
    }

    public string Organization
    {
        get => _infoSettingsService.Organization;
        set => _infoSettingsService.Organization = value;
    }

    public bool IsPersonalDevice
    {
        get => _infoSettingsService.IsPersonalDevice;

        set
        {
            if (_infoSettingsService.IsPersonalDevice == value)
                return;

            _infoSettingsService.IsPersonalDevice = value;
            OnPropertyChanged();

            if (!value)
            {
                // Используйте прямое присваивание без вызова сеттеров
                _infoSettingsService.Patient = string.Empty;
                _infoSettingsService.Birthday = null;
                _infoSettingsService.PregnancyWeek = AppConstants.DefaultCountWeek;
                _infoSettingsService.PregnancyDay = AppConstants.DefaultCountDay;
                _infoSettingsService.PregnancyNumber = 1;

                // Уведомить об изменениях
                OnPropertyChanged(nameof(Patient));
                OnPropertyChanged(nameof(Birthday));
                OnPropertyChanged(nameof(PatientAge));
                OnPropertyChanged(nameof(PregnancyWeek));
                OnPropertyChanged(nameof(PregnancyDay));
                OnPropertyChanged(nameof(PregnancyNumber));
            }
        }
    }

    public string Patient
    {
        get => _infoSettingsService.Patient;

        set
        {
            _infoSettingsService.Patient = value;
            OnPropertyChanged();
        }
    }

    public DateTime? Birthday
    {
        get => _infoSettingsService.Birthday;

        set
        {
            _infoSettingsService.Birthday = value;
            OnPropertyChanged(nameof(PatientAge));
        }
    }

    public string PatientAge
    {
        get
        {
            if (!Birthday.HasValue)
                return AppStrings.Settings_UserBirthdayDescriptionNotSelected;

            var age = DateTools.CalculateAge(Birthday.Value);
            return string.Format(AppStrings.Settings_UserBirthdayDescriptionAge, age);
        }
    }

    public int PregnancyWeek
    {
        get => _infoSettingsService.PregnancyWeek;
        set
        {
            _infoSettingsService.PregnancyWeek = value;
            OnPropertyChanged();
        }
    }

    public int PregnancyDay
    {
        get => _infoSettingsService.PregnancyDay;
        set
        {
            _infoSettingsService.PregnancyDay = value;
            OnPropertyChanged();
        }
    }

    public int PregnancyNumber
    {
        get => _infoSettingsService.PregnancyNumber;
        set
        {
            _infoSettingsService.PregnancyNumber = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<PickerItem<int>> RecordingSpeeds { get; } = new ObservableCollection<PickerItem<int>>
    {
        new PickerItem<int> { Name = string.Format(AppStrings.Settings_PdfRecordingSpeedFormat, 1), Value = 1 },
        new PickerItem<int> { Name = string.Format(AppStrings.Settings_PdfRecordingSpeedFormat, 2), Value = 2 },
        new PickerItem<int> { Name = string.Format(AppStrings.Settings_PdfRecordingSpeedFormat, 3), Value = 3 },
    };

    public PickerItem<int> RecordingSpeed
    {
        get => _recordingSpeed;
        set
        {
            _recordingSpeed = value;
            OnPropertyChanged();

            if (_recordingSpeed == null)
                return;

            _infoSettingsService.PdfRecordingSpeed = _recordingSpeed.Value;
        }
    }

    public double AlarmVolume
    {
        get => _appSettings.SoundLevel * 10.0;
        set
        {
            var settingsValue = value / 10.0;

            if (_appSettings.SoundLevel == settingsValue)
                return;

            _appSettings.SoundLevel = settingsValue;

            _systemVolume.Volume = settingsValue;
            _audioService.Play(Sounds.Attention);

            OnPropertyChanged();
        }
    }
}
