using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace superFans.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Visibility.Visible;
}

public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is false ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is not Visibility.Visible;
}

public class TempColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is float temp)
        {
            if (temp >= 75) return new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36)); // Red
            if (temp >= 60) return new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x07)); // Yellow
            return new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50)); // Green
        }

        return new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xF0)); // Default light
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
