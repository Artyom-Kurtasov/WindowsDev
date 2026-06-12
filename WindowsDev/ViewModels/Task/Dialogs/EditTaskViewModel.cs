using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using System.Windows.Input;
using WindowsDev.Business.Services.TaskService.Interfaces;
using WindowsDev.Dialogs.Interfaces;
using WindowsDev.Domain.TasksModels;
using WindowsDev.Infrastructure;

namespace WindowsDev.ViewModels.Tasks.Dialog
{
    public class EditTaskViewModel : TaskDialogViewModelBase, IDialogViewModel
    {
        private readonly ITaskService _taskService;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly ILogger<EditTaskViewModel> _logger;

        private TasksInfo CurrentTask { get; }

        public EditTaskViewModel(
            TasksInfo currentTask,
            ITaskService taskService,
            IDialogCoordinator dialogCoordinator,
            ILogger<EditTaskViewModel> logger)
        {
            CurrentTask = currentTask ?? throw new ArgumentNullException(nameof(currentTask));
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
            try
            {
                if (!string.IsNullOrWhiteSpace(Name))
                {
                    CurrentTask.Name = Name;
                    CurrentTask.Description = Description;
                    CurrentTask.Priority = Priority;
                    CurrentTask.Progress = Progress;
                    CurrentTask.Status = Status;
                    CurrentTask.DeadLine = DeadLine.ToUniversalTime();

                    await _taskService.UpdateAsync(CurrentTask);

                    if (Completed != null)
                    {
                        await Completed.Invoke();
                    }

                    await Close();
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
                _logger.LogError(ex, "Failed to edit task {taskId}", CurrentTask.Id);
                await _dialogCoordinator.ShowMessageAsync(this,
                    Translate("Error_Title"),
                    Translate("Error_EditTask"),
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
