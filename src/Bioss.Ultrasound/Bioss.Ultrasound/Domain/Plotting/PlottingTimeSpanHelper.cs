using System;

namespace Bioss.Ultrasound.Domain.Plotting
{
    public class PlottingTimeSpanHelper
    {
        private DateTime _startTime = DateTime.Now;

        public TimeSpan CollectTimeSpan(DateTime dateTime)
        {
            return dateTime - _startTime;
        }

        public void Reset(DateTime dateTime)
        {
            _startTime = dateTime;
        }

        public void Reset()
        {
            _startTime = DateTime.Now;
        }
    }
}
