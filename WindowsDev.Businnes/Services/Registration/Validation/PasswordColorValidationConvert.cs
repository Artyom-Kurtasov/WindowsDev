using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace WindowsDev.Business.Services.Registration.Validation
{
    public class PasswordColorValidationConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            if (value is string password && parameter is string ruleType)
            {
                return ruleType switch
                {
                    "Length" => password.Length < 12 ? Brushes.Red : Brushes.Green,
                    "Number" => password.Any(char.IsDigit) ? Brushes.Green : Brushes.Red,
                    "Uppercase" => password.Any(char.IsUpper) ? Brushes.Green : Brushes.Red,
                    "Symbol" => password.Any(c => !char.IsLetterOrDigit(c)) ? Brushes.Green : Brushes.Red
                };
            }
            return Brushes.Red;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}

