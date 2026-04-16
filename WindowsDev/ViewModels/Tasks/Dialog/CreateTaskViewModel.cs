using System.Windows.Input;
using WindowsDev.Business.Services.ProjectService.Interfaces;
using WindowsDev.Business.Services.TaskService.Interfaces;
using WindowsDev.Domain.TasksModels;
using WindowsDev.Infrastructure;
using WindowsDev.ViewModels.Interfaces;

namespace WindowsDev.ViewModels.Tasks.Dialog
{
    public class CreateTaskViewModel : TaskDialogViewModelBase, IInitializableAsync, IProjectDialogCreator
    {
        private readonly ITaskCreator _taskCreator;
        private int _projectId;

        public event Func<Task>? CloseRequested;
        public ICommand CreateTaskCommand { get; }
        public ICommand CancelCommand { get; }
        public CreateTaskViewModel(ITaskCreator taskCreator)
        {
            _taskCreator = taskCreator;

            CreateTaskCommand = new AsyncRelayCommand(CreateTask);
            CancelCommand = new AsyncRelayCommand(Cancel);
        }

        public async Task InitializationAsync(params object[] args)
        {
            _projectId = args.OfType<int>().FirstOrDefault();
        }

        /// <summary>
        /// 
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
                DeadLine = DeadLine.ToUniversalTime(),
                ProjectId = _projectId
            });

            if (CloseRequested != null)
                await CloseRequested.Invoke();
        }

        /// <summary>
        /// Close the dialog without saving.
        /// </summary>
        public async Task Cancel()
        {
            if (CloseRequested != null)
                await CloseRequested.Invoke();
        }
    }
}
