using System.Globalization;
using System.Windows;
using System.Windows.Data;
using WindowsDev.Domain.TasksModels.Enums;

namespace WindowsDev.Business.Services.TaskService.Converters
{
    public class TaskPriorityToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TaskPriority priority)
            {
                string key = $"TaskPriority_{priority}";

                return Application.Current.TryFindResource(key) ?? priority.ToString();
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
