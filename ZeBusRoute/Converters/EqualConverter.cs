using System.Globalization;

namespace ZeBusRoute.Converters;

public class EqualConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int selectedIndex && parameter is string compareString && int.TryParse(compareString, out int compareValue))
        {
            return selectedIndex == compareValue;
        }
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}