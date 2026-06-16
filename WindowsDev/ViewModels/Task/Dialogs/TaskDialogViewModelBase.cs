using WindowsDev.Domain.TasksModels.Enums;
using TaskStatus = WindowsDev.Domain.TasksModels.Enums.TaskStatus;

namespace WindowsDev.ViewModels.Tasks.Dialog
{
    public class TaskDialogViewModelBase : ViewModelBase
    {
        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                _isEditMode = value;
                OnPropertyChanged(nameof(IsEditMode));
            }
        }

        private string _name = null!;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        private string _description = string.Empty;
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        private int _progress;
        public int Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                OnPropertyChanged(nameof(Progress));
            }
        }

        private TaskPriority _priority;
        public TaskPriority Priority
        {
            get => _priority;
            set
            {
                _priority = value;
                OnPropertyChanged(nameof(Priority));
            }
        }

        private TaskStatus _status;
        public TaskStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        private DateTime _deadLine = DateTime.Today;
        public DateTime DeadLine
        {
            get => _deadLine;
            set
            {
                _deadLine = value;
                OnPropertyChanged(nameof(DeadLine));
            }
        }

        // Generates values 0..100 with step 10 for ComboBox
        public IEnumerable<int> Percents =>
            Enumerable.Range(0, 11).Select(x => x * 10);
        public IEnumerable<TaskStatus> Statuses =>
            Enum.GetValues<TaskStatus>().Cast<TaskStatus>();
        public IEnumerable<TaskPriority> Priorities =>
            Enum.GetValues<TaskPriority>().Cast<TaskPriority>();
    }
}
