using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace WindowsDev.Business.Services.Registration.Validation
{
    public class LoginColorValidationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            if (value is string login)
            {
                if (login == "User with this login already exist")
                {
                    return Brushes.Red;
                }
                else
                {
                    return Brushes.Green;
                }
            }
            return Brushes.Red;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo cultureInfo)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}


