using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Bioss.Ultrasound.Data.Database.Entities.Enums;
using Bioss.Ultrasound.Domain.Models;
using Bioss.Ultrasound.Domain.Plotting;
using Bioss.Ultrasound.Resources.Localization;
using Bioss.Ultrasound.Tools;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Xamarin.Forms;

namespace Bioss.Ultrasound.UI.Helpers
{
    public class ChartDrawer
    {
        public const string KEY_FHR = "FHR";
        public const string KEY_TOCO = "TOCO";
        public const string KEY_SPACE_FHR = "SPACE_FHR";
        public const string KEY_SPACE_TOCO = "SPACE_TOCO";

        private readonly GapValueHelper _gapValueHelper = new GapValueHelper();

        private readonly PlottingHelper _plottingHelper;
        private readonly InvalidateHelper _invalidateHelper = new InvalidateHelper();

        private readonly LineSeries _fhrSeries;
        private readonly LineSeries _tocoSeries;
        private readonly Axis _xAxis;
        private readonly PlotModel _model;

        public ChartDrawer(PlottingHelper plottingHelper, bool panEnable = false)
        {
            _plottingHelper = plottingHelper;

            _fhrSeries = ChartHelper.CreateSeries(OxyColors.Blue, KEY_FHR);
            _tocoSeries = ChartHelper.CreateSeries(OxyColors.Green, KEY_TOCO);
            _xAxis = ChartHelper.CreateXTimeSpanAxis(panEnable);
            _model = CreateModel(_fhrSeries, _tocoSeries, _xAxis);
            
            _plottingHelper.ConnectAxis(_xAxis);
        }

        public bool IsPDF { get; set; } = false;

        public PlotModel Model => _model;

        public void Fill(Record record)
        {
            if (!record.Fhrs.Any())
                return;

            var plottingTimeSpanHelper = new PlottingTimeSpanHelper();
            var startTime = record.Fhrs.First().Time;
            plottingTimeSpanHelper.Reset(startTime);

            foreach (var data in record.Fhrs)
            {
                var time = plottingTimeSpanHelper.CollectTimeSpan(data.Time);
                Update(time, data.Fhr, data.Toco);
            }
            var from = plottingTimeSpanHelper.CollectTimeSpan(startTime);
            _plottingHelper.ResetAxisWithMin(from);

            foreach (var e in record.Events)
            {
                //  добавляем на график изображения толчков ребенка
                if (e.Event == Events.FetalMovement)
                    AddFMAnnotation(plottingTimeSpanHelper.CollectTimeSpan(e.Time), IsPDF);

                //  добавляем на график изображения сброса TOCO
                if (e.Event == Events.TocoReset)
                    AddTocoAnnotation(plottingTimeSpanHelper.CollectTimeSpan(e.Time), IsPDF);
            }

            InvalidatePlot();
        }

        public void Update(TimeSpan time, byte fhr, byte toco)
        {
            lock (_model.SyncRoot)
            {
                var dataPointTime = TimeSpanAxis.ToDouble(time);

                _fhrSeries.Points.Add(new DataPoint(dataPointTime, _gapValueHelper.GetValueOrGap(fhr)));
                _tocoSeries.Points.Add(new DataPoint(dataPointTime, toco));
            }
        }

        public void Clear()
        {
            lock (_model.SyncRoot)
            {
                _fhrSeries.Points.Clear();
                _tocoSeries.Points.Clear();
                _model.Annotations.Clear();
                AddGreenSpaceAnnotation(_model);
                AddEmptySpaceAnnotation(_model);
            }
        }

        public (byte Fhr, byte Toco) GetValue(double xPosition)
        {
            var fhrSeries = (LineSeries) _model.Series.First(a => a is LineSeries series && series.YAxisKey == KEY_FHR);
            var tocoSeries = (LineSeries)_model.Series.First(a => a is LineSeries series && series.YAxisKey == KEY_TOCO);

            var fhr = (byte)GetYbyX(xPosition, fhrSeries.Points);
            var toco = (byte)GetYbyX(xPosition, tocoSeries.Points);

            return (fhr, toco);
        }

        public void ResetFhrMinMax(int min, int max)
        {
            var spaceFhr = _model.Axes.First(a => a.Key == KEY_SPACE_FHR);
            var fhr = _model.Axes.First(a => a.Key == KEY_FHR);
            var spaceToco = _model.Axes.First(a => a.Key == KEY_SPACE_TOCO);
            var toco = _model.Axes.First(a => a.Key == KEY_TOCO);

            ChartHelper.ResetAxisRange(fhr, min, max);

            AlignAxes(spaceFhr, fhr, spaceToco, toco);
        }

        public void AddTocoAnnotation(TimeSpan timeSpan, bool isPdf = false)
        {
            //  [FIX] в генерации PDF есть ошибка, которая рисует иконки вверх ногами
            //  по этому для PDF рисуем иконку вверх ногами
            var imageName = isPdf
                ? "pin_top"
                : "pin";

            var width = isPdf
                ? 10
                : 20;

            _model.Annotations.Add(OxyTools.MakeImageAnnotation(TimeSpanAxis.ToDouble(timeSpan), 5, imageName, KEY_SPACE_TOCO, width));
        }

        public void AddFMAnnotation(TimeSpan timeSpan, bool isPdf = false)
        {
            //  [FIX] в генерации PDF есть ошибка, которая рисует иконки вверх ногами
            //  по этому для PDF рисуем иконку вверх ногами
            var imageName = isPdf
                ? "pin_top"
                : "pin";

            var width = isPdf
                ? 10
                : 20;

            _model.Annotations.Add(OxyTools.MakeImageAnnotation(TimeSpanAxis.ToDouble(timeSpan), 5, imageName, KEY_SPACE_FHR, width));
        }

        public void InvalidatePlot(bool optimization = false)
        {
            if (optimization && !_invalidateHelper.NeedInvalidate())
                return;

            try
            {
                _model.InvalidatePlot(true);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }
        }

        private PlotModel CreateModel(LineSeries fhrSeries, LineSeries tocoSeries, Axis xAxis)
        {
            var spaceYFhr = ChartHelper.CreateYAxis("", 0, 10, KEY_SPACE_FHR);
            spaceYFhr.IsAxisVisible = false;
            //
            var fhrYAxis = ChartHelper.CreateYAxis(AppStrings.Chart_FHRAxysTitle, 50, 220, fhrSeries.YAxisKey);
            //
            var spaceYToco = ChartHelper.CreateYAxis("", 0, 20, KEY_SPACE_TOCO);
            spaceYToco.IsAxisVisible = false;
            //
            var tocoYAxis = ChartHelper.CreateYAxis(AppStrings.Chart_TOCOAxysTitle, 0, 100, tocoSeries.YAxisKey);

            //  Рассчет позиции осей в процентах
            AlignAxes(spaceYFhr, fhrYAxis, spaceYToco, tocoYAxis);
            
            //
            var model = new PlotModel();
            model.PlotAreaBorderColor = OxyColors.Transparent;
            
            model.Axes.Add(fhrYAxis);
            model.Series.Add(fhrSeries);
            AddGreenSpaceAnnotation(model);

            model.Axes.Add(tocoYAxis);
            model.Series.Add(tocoSeries);

            model.Axes.Add(spaceYToco);
            model.Axes.Add(spaceYFhr);
            AddEmptySpaceAnnotation(model);

            model.Axes.Add(xAxis);

            return model;
        }

        private void AlignAxes(Axis spaceYFhr, Axis fhrYAxis, Axis spaceYToco, Axis tocoYAxis)
        {
            var axes = new List<Axis> { spaceYFhr, fhrYAxis, spaceYToco, tocoYAxis };
            var sum = axes.Sum(a => a.Maximum - a.Minimum);

            //
            tocoYAxis.StartPosition = 0;
            tocoYAxis.EndPosition = (tocoYAxis.Maximum - tocoYAxis.Minimum) / sum;

            //
            spaceYToco.StartPosition = tocoYAxis.EndPosition;
            spaceYToco.EndPosition = spaceYToco.StartPosition + (spaceYToco.Maximum - spaceYToco.Minimum) / sum;

            //
            fhrYAxis.StartPosition = spaceYToco.EndPosition;
            fhrYAxis.EndPosition = fhrYAxis.StartPosition + (fhrYAxis.Maximum - fhrYAxis.Minimum) / sum;

            //
            spaceYFhr.StartPosition = fhrYAxis.EndPosition;
            spaceYFhr.EndPosition = spaceYFhr.StartPosition + (spaceYFhr.Maximum - spaceYFhr.Minimum) / sum;

        }

        private void AddEmptySpaceAnnotation(PlotModel model)
        {
            var spaceToco = model.Axes.First(a => a.Key == KEY_SPACE_TOCO);
            ChartHelper.AddRectangleAnnotation(model, spaceToco.Minimum, spaceToco.Maximum, OxyColor.Parse("#FFFFFF"), KEY_SPACE_TOCO);

            var spaceFhr = model.Axes.First(a => a.Key == KEY_SPACE_FHR);
            ChartHelper.AddRectangleAnnotation(model, spaceFhr.Minimum, spaceFhr.Maximum, OxyColor.Parse("#FFFFFF"), KEY_SPACE_FHR);
        }

        private void AddGreenSpaceAnnotation(PlotModel model)
        {
            ChartHelper.AddRectangleAnnotation(model, 110, 160, OxyColor.Parse("#66C1F7BE"), KEY_FHR);
        }

        private double GetYbyX(double xPosition, List<DataPoint> points)
        {
            if (points.Count < 2)
                return 0;

            var number = xPosition;
            var point = points.Aggregate((point1, point2) => Math.Abs(point1.X - number) < Math.Abs(point2.X - number) ? point1 : point2);
            return point.Y;
        }
    }

    class GapValueHelper
    {
        private const double GapStep = 20.0;
        private double _lastValue;

        //  TODO: Не придумал нормальное название.
        //  Функция возвращает NAN для всех случаев, когда нужно сделать разрыв графика
        //  например, когда значение Y == 0
        public double GetValueOrGap(double value)
        {
            var returnValue = value;

            if (value == 0)
                returnValue = double.NaN;

            if (Math.Abs(value - _lastValue) > GapStep)
                returnValue = double.NaN;

            _lastValue = value;

            return returnValue;
        }
    }

    class InvalidateHelper
    {
        // Класс нужен для того, чтоб сообщить, как часть нужно обновлять график
        // очень частое обновление сильно нагружает устройство
        // очень частое обновление на андроид приводит к збою приложения

        private readonly int _waitStep;
        private int _count;

        public InvalidateHelper()
        {
            _waitStep = Device.RuntimePlatform == Device.Android
                ? 10
                : 2;
        }

        public bool NeedInvalidate()
        {
            _count++;
            if (_count < _waitStep)
                return false;

            _count = 0;
            return true;
        }
    }
}
