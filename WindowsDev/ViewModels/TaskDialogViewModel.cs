using System;
using System.Threading.Tasks;
using System.Windows.Input;
using WindowsDev.Businnes.Services;
using WindowsDev.Businnes.Services.ProjectService.Interfaces;
using WindowsDev.Businnes.Services.TaskService.Interfaces;
using WindowsDev.Domain.UsersAuthInfo;
using WindowsDev.Infrastructure;

namespace WindowsDev.ViewModels
{
    /// <summary>
    /// ViewModel for creating and editing tasks.
    /// Handles task properties, creation, editing, and dialog closure.
    /// </summary>
    public class TaskDialogViewModel : ViewModelBase, IProjectDialogCreator
    {
        private readonly SharedDataService _sharedDataService;
        private readonly ITaskLoader _taskLoader;
        private readonly ITaskCreator _taskCreator;
        private readonly ITaskWriter _taskWriter;

        public event Func<Task> Close;

        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set { _isEditMode = value; OnPropertyChanged(nameof(IsEditMode)); }
        }

        private int _taskId;

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
            set { _deadLine = value; OnPropertyChanged(nameof(DeadLine)); }
        }

        public ICommand CancelCommand { get; }
        public ICommand CreateTaskCommand { get; }
        public ICommand EditTaskCommand { get; }

        /// <summary>
        /// Constructor for TaskDialogViewModel.
        /// </summary>
        public TaskDialogViewModel(
            ITaskCreator taskCreator,
            ITaskLoader taskLoader,
            SharedDataService sharedDataService,
            ITaskWriter taskWriter)
        {
            _taskCreator = taskCreator;
            _taskLoader = taskLoader;
            _sharedDataService = sharedDataService;
            _taskWriter = taskWriter;

            CreateTaskCommand = new AsyncRelayCommand(CreateTask);
            EditTaskCommand = new AsyncRelayCommand(EditTask);
            CancelCommand = new AsyncRelayCommand(Cancel);
        }

        /// <summary>
        /// Create a new task and refresh task list.
        /// </summary>
        public async Task CreateTask()
        {
            await _taskCreator.CreateTask(new TaskDTO
            {
                Name = Name,
                Description = Description,
                Priority = Priority,
                Progress = Progress,
                Status = Status,
                DeadLine = DeadLine
            });

            if (Close != null)
                await Close.Invoke();

            _sharedDataService.TaskList = await _taskLoader.LoadTaskAsync();
        }

        /// <summary>
        /// Edit existing task and refresh shared data.
        /// </summary>
        private async Task EditTask()
        {
            var existedTask = _sharedDataService.CurrentTask;

            if (existedTask != null)
            {
                existedTask.Id = _taskId;
                existedTask.Name = Name;
                existedTask.Description = Description;
                existedTask.Priority = Priority;
                existedTask.Progress = Progress;
                existedTask.Status = Status;
                existedTask.DeadLine = DeadLine;

                await _taskWriter.UpdateAsync(existedTask);

                _sharedDataService.OnPropertyChanged(nameof(SharedDataService.CurrentTask));

                if (Close != null)
                    await Close.Invoke();

                IsEditMode = false;
            }
        }

        /// <summary>
        /// Close the dialog without saving.
        /// </summary>
        public async Task Cancel()
        {
            if (Close != null)
                await Close.Invoke();
        }

        /// <summary>
        /// Populate the ViewModel with existing task data for editing.
        /// </summary>
        public void Edit(object? taskObj)
        {
            if (taskObj is not TasksInfo task)
                return;

            _taskId = task.Id;
            Name = task.Name;
            Description = task.Description;
            Priority = task.Priority;
            Progress = task.Progress;
            Status = task.Status;
            DeadLine = task.DeadLine;

            IsEditMode = true;
        }
    }
}