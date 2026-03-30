using System.Globalization;

namespace yBook.Converters;

public class BoolToIconColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => (bool)value ? Colors.Teal : Colors.Gray;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}