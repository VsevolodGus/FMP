using System.Globalization;

namespace Bioss.Ultrasound.Maui.Converters;

public class LossPercentageToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var val = (double)value;

        if (val <= 10)
            return Colors.Black;

        if (val <= 25)
            return Colors.Gold;

        return Colors.Red;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
