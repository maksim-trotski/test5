using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace _5Elem.Client.Converters
{
    public class BooleanToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isLoginMode = value is bool boolValue && boolValue;
            var targetBrush = parameter as System.Windows.Media.Brush;

            return isLoginMode ? targetBrush : Application.Current.FindResource("TextSecondaryBrush");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
