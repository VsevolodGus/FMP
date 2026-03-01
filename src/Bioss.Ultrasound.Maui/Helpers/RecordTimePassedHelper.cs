namespace Bioss.Ultrasound.Maui.Helpers;

internal class RecordTimePassedHelper
{
    public bool IsAutoRecord { get; set; }
    public int TotalAutoRecordSeconds { get; set; }
    public DateTime StartTime { get; set; }

    public void Init(bool isAutoRecord, int totalAutoRecordSeconds, DateTime startTime)
    {
        IsAutoRecord = isAutoRecord;
        TotalAutoRecordSeconds = totalAutoRecordSeconds;
        StartTime = startTime;
    }

    public TimeSpan CurrentRecordTime => DateTime.Now - StartTime;

    public bool IsTimeEnd
    {
        get
        {
            if (!IsAutoRecord)
                return false;

            return (DateTime.Now - StartTime).TotalSeconds > TotalAutoRecordSeconds;
        }
    }

    public string DisplayTimePassed()
    {
        var timeLeft = (DateTime.Now - StartTime).TotalSeconds;

        if (IsAutoRecord)
        {
            var tt = TotalAutoRecordSeconds - timeLeft;
            var span = TimeSpan.FromSeconds(tt);
            return $"{span.Minutes:D2}:{span.Seconds:D2}";
        }
        else
        {
            var span = TimeSpan.FromSeconds(timeLeft);
            return $"{span.Minutes:D2}:{span.Seconds:D2}";
        }
    }
}
