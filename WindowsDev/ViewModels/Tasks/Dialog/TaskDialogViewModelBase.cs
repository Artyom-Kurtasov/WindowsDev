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

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        private string _description = string.Empty;
        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(nameof(Description)); }
        }

        private string _progress = string.Empty;
        public string Progress
        {
            get => _progress;
            set { _progress = value; OnPropertyChanged(nameof(Progress)); }
        }

        private string _priority = string.Empty;
        public string Priority
        {
            get => _priority;
            set { _priority = value; OnPropertyChanged(nameof(Priority)); }
        }

        private string _status = string.Empty;
        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(nameof(Status)); }
        }

        private DateTime _deadLine;
        public DateTime DeadLine
        {
            get => _deadLine;
            set
            {
                _deadLine = value;
                OnPropertyChanged(nameof(DeadLine));
            }
        }
    }
}
