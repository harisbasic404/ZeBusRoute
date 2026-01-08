using System.Globalization;

namespace ZeBusRoute.Converters;

public class TabColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int selectedIndex && parameter is string tabIndexString && int.TryParse(tabIndexString, out int tabIndex))
        {
            return selectedIndex == tabIndex ? Color.FromArgb("#8BC34A") : Color.FromArgb("#9E9E9E");
        }
        return Color.FromArgb("#9E9E9E");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}