using System.Globalization;
using System.Windows.Data;
using WindowsDev.Domain.Enums;

namespace WindowsDev.Converters
{
    public class TaskPriorityToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TaskPriority priority)
            {
                string key = $"Task_Priority_{priority}";

                return App.Current.TryFindResource(key) ?? priority.ToString();
            }

            return "";
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
    }
}
