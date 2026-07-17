using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace WindowsDev.Business.Services.SettingsManager
{
    public class EnumDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Enum enumValue)
            {
                return GetEnumDescription(enumValue);
            }

            return value;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            throw new NotImplementedException();
        }

        private string GetEnumDescription(Enum enumValue)
        {
            FieldInfo field = enumValue.GetType().GetField(enumValue.ToString());

            var attribute = field.GetCustomAttribute<DescriptionAttribute>();

            return attribute?.Description ?? enumValue.ToString();
        }
    }
}
