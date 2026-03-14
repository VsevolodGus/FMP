using System;

namespace Bioss.Ultrasound.UI.Helpers
{
    public sealed class GapValueHelper
    {
        private const double GapStep = 20.0;
        private double _lastValue;

        //  TODO: Не придумал нормальное название.
        //  Функция возвращает NAN для всех случаев, когда нужно сделать разрыв графика
        //  например, когда значение Y == 0
        public double GetValueOrGap(in double value)
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
}
