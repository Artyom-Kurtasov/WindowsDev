using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using System.Windows.Input;
using WindowsDev.Business.Services.TaskService.Interfaces;
using WindowsDev.Dialogs.Interfaces;
using WindowsDev.Domain.TasksModels;
using WindowsDev.Infrastructure;

namespace WindowsDev.ViewModels.Tasks.Dialog
{
    public class CreateTaskViewModel : TaskDialogViewModelBase, IDialogViewModel
    {
        private readonly ITaskService _taskService;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly ILogger<CreateTaskViewModel> _logger;
        private readonly int _projectId;

        public CreateTaskViewModel(int projectId,
            ITaskService taskService,
            IDialogCoordinator dialogCoordinator,
            ILogger<CreateTaskViewModel> logger)
        {
            if (projectId < 1)
                throw new ArgumentOutOfRangeException(nameof(projectId));

            _projectId = projectId;
            _taskService = taskService;
            _dialogCoordinator = dialogCoordinator;
            _logger = logger;

            CreateTaskCommand = new AsyncRelayCommand(CreateTask);
            CancelCommand = new AsyncRelayCommand(Cancel);
        }

        public ICommand CreateTaskCommand { get; }
        public ICommand CancelCommand { get; }

        public event Func<Task>? CloseRequested;
        public event Func<Task>? Completed;

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

                    if (Completed != null)
                    {
                        await Completed.Invoke();
                    }

                    await Cancel();
                }
                else
                {
                    await _dialogCoordinator.ShowMessageAsync(this,
                        Translate("Warning_Title"),
                        Translate("Warning_EnterName"),
                        MessageDialogStyle.Affirmative);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create task in project {ProjectId}", _projectId);
                await _dialogCoordinator.ShowMessageAsync(this,
                    Translate("Error_Title"),
                    Translate(ex.Message),
                    MessageDialogStyle.Affirmative);
            }
        }

        public async Task Cancel()
        {
            if (CloseRequested != null)
            {
                await CloseRequested.Invoke();
            }
        }
    }
}
