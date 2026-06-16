using System.Globalization;
using System.Windows;
using System.Windows.Data;
using TaskStatus = WindowsDev.Domain.TasksModels.Enums.TaskStatus;

namespace WindowsDev.Business.Services.TaskService.Converters
{
    public class TaskStatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TaskStatus status)
            {
                string key = $"TaskStatus_{status}";

                return Application.Current.TryFindResource(key) ?? status.ToString();
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
