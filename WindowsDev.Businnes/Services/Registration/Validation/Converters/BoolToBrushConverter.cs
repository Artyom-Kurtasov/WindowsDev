using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace WindowsDev.Business.Services.Registration.Validation.Converters
{
    public class BoolToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            return value is bool isValid && isValid ? Brushes.Green : Brushes.Red;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}


