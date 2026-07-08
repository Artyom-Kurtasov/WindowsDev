using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using System.Windows.Input;
using WindowsDev.Business.Services.TaskService.Interfaces;
using WindowsDev.Dialogs.Interfaces;
using WindowsDev.Domain;
using WindowsDev.Domain.DialogsMessages.Errors;
using WindowsDev.Domain.DialogsMessages.Warnings;
using WindowsDev.Domain.TasksModels;
using WindowsDev.Infrastructure;
using WindowsDev.Infrastructure.Logging;

namespace WindowsDev.ViewModels.Tasks.Dialog
{
    public class EditTaskViewModel : TaskDialogViewModelBase, IDialogViewModel
    {
        private readonly ITaskService _taskService;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly ILogger<EditTaskViewModel> _logger;

        private TasksInfo CurrentTask { get; }

        public EditTaskViewModel(TasksInfo currentTask,
            ITaskService taskService,
            IDialogCoordinator dialogCoordinator,
            ILogger<EditTaskViewModel> logger)
        {
            CurrentTask = currentTask;
            _taskService = taskService;
            _dialogCoordinator = dialogCoordinator;
            _logger = logger;

            EditTaskCommand = new AsyncRelayCommand(EditTask);
            SetEditDialog();
        }

        public ICommand EditTaskCommand { get; }

        public event Func<Task>? CloseRequested;
        public event Func<Task>? Completed;

        private async Task EditTask()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                await _dialogCoordinator.ShowMessageAsync(this,
                    Translate(DialogTitles.Warning),
                    Translate(TaskDialogWarnings.EnterName),
                    MessageDialogStyle.Affirmative);
                return;
            }

            try
            {
                CurrentTask.Name = Name;
                CurrentTask.Description = Description;
                CurrentTask.Priority = Priority;
                CurrentTask.Progress = Progress;
                CurrentTask.Status = Status;
                CurrentTask.DeadLine = DeadLine.ToUniversalTime();

                await _taskService.UpdateAsync(CurrentTask);

                Completed?.Invoke();
            }
            catch (Exception ex)
            {
                TaskLogs.TaskUpdateFailed(_logger, CurrentTask.Id, ex);
                await _dialogCoordinator.ShowMessageAsync(this,
                    Translate(DialogTitles.Error),
                    Translate(CommonErrors.UnexpectedError),
                    MessageDialogStyle.Affirmative);
            }
        }

        private void SetEditDialog()
        {
            Name = CurrentTask.Name;
            Description = CurrentTask.Description!;
            Priority = CurrentTask.Priority;
            Progress = CurrentTask.Progress;
            Status = CurrentTask.Status;
            DeadLine = CurrentTask.DeadLine;

            IsEditMode = true;
        }

        private async Task Close()
        {
            if (CloseRequested != null)
            {
                await CloseRequested.Invoke();
            }

            IsEditMode = false;
        }
    }
}
