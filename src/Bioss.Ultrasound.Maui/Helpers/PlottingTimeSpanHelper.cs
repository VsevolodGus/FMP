namespace Bioss.Ultrasound.Maui.Helpers;

public class PlottingTimeSpanHelper
{
    private DateTime _startTime = DateTime.Now;

    public TimeSpan CollectTimeSpan(in DateTime dateTime)
    {
        return dateTime - _startTime;
    }

    public void Reset(in DateTime dateTime)
    {
        _startTime = dateTime;
    }

    public void Reset()
    {
        _startTime = DateTime.Now;
    }
}
