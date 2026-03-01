namespace Bioss.Ultrasound.Core.Services;

public class AppSettingsService
{
    private const int DefaultChartYMin = 50;
    private const int DefaultChartYMax = 220;
    private const int DefaultRecordTimeMinutes = 20; // 20 мин

    private const string NAME = "AppSettings";

    private const string AUTO_RESET_TOCO = "isAutoResetToco";
    private const string AUTO_CONNECT = "isAutoConnect";
    private const string CHART_X_SCALE_SECONDS = "chartXScaleSeconds";
    private const string CHART_Y_MINIMUM = "chartYMinimum";
    private const string CHART_Y_MAXIMUM = "chartYMaximum";
    private const string AUTO_RECORD_TIME = "isAutoRecordTime";
    private const string RECORD_TIME_MINUTES = "recordTimeMinutes";
    private const string CONFIRM_RECORD_COMPLETE = "confirmRecordCompleated";
    private const string AUTO_COMPLETE_RECORD_BY_CRITERIA = "autoCompleteRecordByCriteria";
    private const string IS_BATTERY_LOW_SOUND = "isBatteryLowSound";
    private const string IS_LOSS_DATA_SOUND = "isLossDataSound";
    private const string SOUND_LEVEL = "soundLevel";

    public bool IsAutoResetToco
    {
        get => Preferences.Get(AUTO_RESET_TOCO, false, NAME);
        set => Preferences.Set(AUTO_RESET_TOCO, value, NAME);
    }

    public bool IsAutoConnect
    {
        get => Preferences.Get(AUTO_CONNECT, false, NAME);
        set => Preferences.Set(AUTO_CONNECT, value, NAME);
    }

    public int ChartXScaleSeconds
    {
        get => Preferences.Get(CHART_X_SCALE_SECONDS, AppConstants.DurationSeconsDefault, NAME);
        set => Preferences.Set(CHART_X_SCALE_SECONDS, value, NAME);
    }

    public int ChartYMinimum
    {
        get => Preferences.Get(CHART_Y_MINIMUM, DefaultChartYMin, NAME);
        set => Preferences.Set(CHART_Y_MINIMUM, value, NAME);
    }

    public int ChartYMaximum
    {
        get => Preferences.Get(CHART_Y_MAXIMUM, DefaultChartYMax, NAME);
        set => Preferences.Set(CHART_Y_MAXIMUM, value, NAME);
    }

    public bool IsAutoRecordTime
    {
        get => Preferences.Get(AUTO_RECORD_TIME, false, NAME);
        set => Preferences.Set(AUTO_RECORD_TIME, value, NAME);
    }

    public int RecordTimeMinutes
    {
        get => Preferences.Get(RECORD_TIME_MINUTES, DefaultRecordTimeMinutes, NAME);
        set => Preferences.Set(RECORD_TIME_MINUTES, value, NAME);
    }

    public bool IsConfirmRecordCompleated
    {
        get => Preferences.Get(CONFIRM_RECORD_COMPLETE, false, NAME);
        set => Preferences.Set(CONFIRM_RECORD_COMPLETE, value, NAME);
    }

    public bool IsAutoCompleteRecordByCriteria
    {
        get => Preferences.Get(AUTO_COMPLETE_RECORD_BY_CRITERIA, false, NAME);
        set => Preferences.Set(AUTO_COMPLETE_RECORD_BY_CRITERIA, value, NAME);
    }

    public bool IsBatteryLowSound
    {
        get => Preferences.Get(IS_BATTERY_LOW_SOUND, true, NAME);
        set => Preferences.Set(IS_BATTERY_LOW_SOUND, value, NAME);
    }

    public bool IsLossDataSound
    {
        get => Preferences.Get(IS_LOSS_DATA_SOUND, true, NAME);
        set => Preferences.Set(IS_LOSS_DATA_SOUND, value, NAME);
    }

    public double SoundLevel
    {
        get => Preferences.Get(SOUND_LEVEL, 0.5, NAME);
        set => Preferences.Set(SOUND_LEVEL, value, NAME);
    }
}
