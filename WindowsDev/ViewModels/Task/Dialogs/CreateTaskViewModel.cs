using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using WindowsDev.Business.Services.Localization.Interfaces;
using WindowsDev.Business.Services.TaskService.Interfaces;
using WindowsDev.Dialogs.Interfaces;
using WindowsDev.Domain;
using WindowsDev.Domain.DialogsMessages.Errors;
using WindowsDev.Domain.TasksModels;
using WindowsDev.Infrastructure;
using WindowsDev.Infrastructure.Logging;

namespace WindowsDev.ViewModels.Tasks.Dialog
{
    public class CreateTaskViewModel : TaskDialogViewModelBase, IDialogViewModel
    {
        private readonly ITaskService _taskService;
        private readonly ILogger<CreateTaskViewModel> _logger;
        private readonly int _projectId;

        public CreateTaskViewModel(
            int projectId,
            ITaskService taskService,
            IDialogCoordinator dialogCoordinator,
            ILogger<CreateTaskViewModel> logger,
            ILanguageChanger languageChanger
        )
            : base(languageChanger, dialogCoordinator)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(projectId);

            _projectId = projectId;
            _taskService = taskService;
            _logger = logger;

            CreateTaskCommand = new AsyncRelayCommand(CreateTaskAsync);
            CancelCommand = new AsyncRelayCommand(CancelAsync);
        }

        public ICommand CreateTaskCommand { get; }
        public ICommand CancelCommand { get; }

        public event Func<Task>? CloseRequested;
        public event Func<Task>? Completed;

        private async Task CreateTaskAsync()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                await ShowWarningAsync();
                return;
            }

            try
            {
                await _taskService.AddAsync(
                    new TasksInfo
                    {
                        Name = Name,
                        Description = Description,
                        Priority = Priority,
                        Progress = Progress,
                        Status = Status,
                        DeadLine = DeadLine.ToUniversalTime(),
                        CreatedAt = DateTime.UtcNow,
                        ProjectId = _projectId,
                    }
                );

                if (Completed != null)
                {
                    Completed?.Invoke();
                    await CancelAsync();
                }
            }
            catch (Exception ex)
            {
                TaskLogs.TaskCreationFailed(_logger, Name, _projectId, ex);

                await _dialogCoordinator.ShowMessageAsync(
                    this,
                    Translate(DialogTitles.Error),
                    Translate(CommonErrors.UnexpectedError),
                    MessageDialogStyle.Affirmative
                );
            }
        }

        private async Task CancelAsync()
        {
            if (CloseRequested != null)
                await CloseRequested.Invoke();
        }
    }
}
