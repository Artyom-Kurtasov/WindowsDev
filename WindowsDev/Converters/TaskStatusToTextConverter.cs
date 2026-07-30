using System.Globalization;
using System.Windows.Data;
using TaskStatus = WindowsDev.Domain.Enums.TaskStatus;

namespace WindowsDev.Converters
{
    public class TaskStatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TaskStatus status)
            {
                string key = $"Task_Status_{status}";

                return App.Current.TryFindResource(key) ?? status.ToString();
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
