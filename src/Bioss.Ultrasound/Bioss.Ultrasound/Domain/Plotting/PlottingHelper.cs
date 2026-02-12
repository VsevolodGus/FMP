using System;
using System.Collections.Generic;
using Bioss.Ultrasound.UI.Helpers;
using OxyPlot.Axes;

namespace Bioss.Ultrasound.Domain.Plotting
{
    public class PlottingHelper
    {
        private List<Axis> _xAxes = new List<Axis>();

        public const int DurationSeconsDefault = 120;

        /// <summary>
        /// Масштаб графика в секундах по оси X
        /// </summary>
        public double Scale { get; set; } = DurationSeconsDefault;

        public void ConnectAxis(Axis xAxis)
        {
            _xAxes.Add(xAxis);
        }

        public void ResetAxisWithMax(in TimeSpan time)
        {
            var startTime = time.Add(TimeSpan.FromSeconds(-Scale));
            ResetDateTimeAxisRange(startTime, time);
        }

        public void ResetAxisWithMin(in TimeSpan time)
        {
            var endTime = time.Add(TimeSpan.FromSeconds(Scale));
            ResetDateTimeAxisRange(time, endTime);
        }

        private void ResetDateTimeAxisRange(in TimeSpan from, in TimeSpan to)
        {
            foreach(var axis in _xAxes)
                ChartHelper.ResetDateTimeAxisRange(axis, from, to);
        }
    }
}
