using System;
using System.Globalization;
using Xamarin.Forms;

namespace Bioss.Ultrasound.UI.Converters
{
    public class SoundLevelToPercentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (double)value;
            var percent = val * 100;
            return $"{percent}%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
