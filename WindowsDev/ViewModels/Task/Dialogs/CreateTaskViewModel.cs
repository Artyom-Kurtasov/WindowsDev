using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
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
        private readonly ITaskService _taskService;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly ILogger<CreateTaskViewModel> _logger;

        private int _projectId;

        public CreateTaskViewModel(ITaskService taskService, IDialogCoordinator dialogCoordinator,
            ILogger<CreateTaskViewModel> logger)
        {
            _taskService = taskService;
            _dialogCoordinator = dialogCoordinator;
            _logger = logger;

            CreateTaskCommand = new AsyncRelayCommand(CreateTask);
            CancelCommand = new AsyncRelayCommand(Cancel);
        }

        // Commands
        public ICommand CreateTaskCommand { get; }
        public ICommand CancelCommand { get; }

        // Events
        public event Func<Task>? CloseRequested;
        public event Func<Task>? Completed;

        // Init
        public async Task InitializationAsync(params object[] args)
        {
            var projectId = args.OfType<int>().FirstOrDefault();

            if (projectId < 1)
                throw new ArgumentNullException();

            _projectId = projectId;
        }

        // Commands logic
        public async Task CreateTask()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(Name))
                {
                    await _taskService.AddAsync(new TasksInfo
                    {
                        Name = Name,
                        Description = Description,
                        Priority = Priority,
                        Progress = Progress,
                        Status = Status,
                        DeadLine = DeadLine.ToUniversalTime(),
                        CreatedAt = DateTime.UtcNow,
                        ProjectId = _projectId
                    });

                    Completed?.Invoke();

                    await Cancel();
                }
                else
                {
                    await _dialogCoordinator.ShowMessageAsync(this,
                        "Warning",
                        "Enter a name",
                        MessageDialogStyle.Affirmative);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create task\n {mes}", ex.Message);
                await _dialogCoordinator.ShowMessageAsync(this,
                    "Error",
                    "Failed to create task",
                    MessageDialogStyle.Affirmative);
            }
        }

        public async Task Cancel()
        {
            if (CloseRequested != null)
                await CloseRequested.Invoke();
        }
    }
}