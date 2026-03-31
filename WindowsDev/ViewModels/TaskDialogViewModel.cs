using System.Windows.Input;
using WindowsDev.Businnes.Services;
using WindowsDev.Businnes.Services.ProjectService.Interfaces;
using WindowsDev.Businnes.Services.TaskService.Interfaces;
using WindowsDev.Domain.UsersAuthInfo;
using WindowsDev.Infrastructure;

namespace WindowsDev.ViewModels
{
    public class TaskDialogViewModel : ViewModelBase, IProjectDialogCreator
    {
        private readonly SharedDataService _sharedDataService;
        private readonly ITaskLoader _taskLoader;
        private readonly ITaskCreator _taskCreator;

        public event Func<Task> Close;

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        private string _progress;
        public string Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                OnPropertyChanged();
            }
        }

        private string _priority;
        public string Priority
        {
            get => _priority;
            set
            {
                _priority = value;
                OnPropertyChanged();
            }
        }

        private string _status;
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        private DateTime _deadLine;
        public DateTime DeadLine
        {
            get => _deadLine;
            set
            {
                _deadLine = value;
                OnPropertyChanged();
            }
        }

        public ICommand CreateTaskCommand { get; }

        public TaskDialogViewModel(ITaskCreator taskCreator, ITaskLoader taskLoader, SharedDataService sharedDataService)
        {
            _taskCreator = taskCreator;
            _taskLoader = taskLoader;
            _sharedDataService = sharedDataService;

            CreateTaskCommand = new AsyncRelayCommand(CreateTaskExecute);
        }

        public async Task CreateTaskExecute()
        {
            await _taskCreator.CreateTask(new TaskDTO
            {
                Name = this.Name,
                Description = this.Description,
                Priority = this.Priority,
                Progress = this.Progress,
                Status = this.Status,
                DeadLine = this.DeadLine
            });

            if (Close != null)
                await Close.Invoke();

            _sharedDataService.TaskList = await _taskLoader.LoadTaskAsync();
        }
    }
}
