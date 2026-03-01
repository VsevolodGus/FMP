using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Bioss.Ultrasound.Maui.Helpers;

public class ChartHelper
{
    public static LinearAxis CreateYAxis(in string title, in double minimum, in double maximum, in string key = null)
    {
        return new LinearAxis
        {
            Position = AxisPosition.Left,
            IsPanEnabled = false,
            IsZoomEnabled = false,
            MajorGridlineStyle = LineStyle.Solid,
            MinorGridlineStyle = LineStyle.Solid,
            MinorGridlineThickness = 0,
            Minimum = minimum,
            Maximum = maximum,
            MajorStep = 10,
            Title = title,
            Key = key
        };
    }

    public static TimeSpanAxis CreateXTimeSpanAxis(in bool panEnable)
    {
        return new TimeSpanAxis
        {
            Position = AxisPosition.Bottom,
            IsPanEnabled = panEnable,
            IsZoomEnabled = false,
            MajorGridlineStyle = LineStyle.Solid,
            MinorGridlineStyle = LineStyle.Solid,
            MajorStep = 60, //  1 минута
            MinorStep = 10, //  10 сек
            StringFormat = "h:mm"
        };
    }

    public static LineSeries CreateSeries(in OxyColor color, string yAxisKey = null)
    {
        return new LineSeries
        {
            LineStyle = LineStyle.Solid,
            Color = color,
            StrokeThickness = 2,
            YAxisKey = yAxisKey,
            CanTrackerInterpolatePoints = false,
            TrackerFormatString = string.Empty
        };
    }

    public static void ResetAxisRange(Axis axis, in int from, in int to)
    {
        //axis.Reset();
        axis.Minimum = from;
        axis.Maximum = to;
    }

    public static void ResetDateTimeAxisRange(Axis axis, in TimeSpan from, in TimeSpan to)
    {
        //axis.Reset();
        axis.Minimum = TimeSpanAxis.ToDouble(from);
        axis.Maximum = TimeSpanAxis.ToDouble(to);
    }

    public static void DeleteNotViewData(LineSeries series, in double scale)
    {
        //  scale - количество секунд на графике.
        //  данные приходят 4 раза в секунду
        var count = scale * 4;

        if (series.Points.Count >= count)
            series.Points.RemoveAt(0);
    }

    public static void AddRectangleAnnotation(PlotModel model, in double minY, in double maxY, OxyColor color, string key)
    {
        var spaceRectangle = new RectangleAnnotation()
        {
            MinimumY = minY,
            MaximumY = maxY,
            Fill = color,
            Stroke = color,
            YAxisKey = key
        };

        model.Annotations.Add(spaceRectangle);
    }
}
