using System;
using System.Globalization;
using Xamarin.Forms;

namespace Bioss.Ultrasound.UI.Converters
{
    public class LossPercentageToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (double)value;

            if (val <= 10)
                return Color.Black;

            if (val <= 25)
                return Color.Gold;

            return Color.Red;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
