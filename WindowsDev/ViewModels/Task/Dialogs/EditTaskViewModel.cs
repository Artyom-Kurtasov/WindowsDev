using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using System.Windows.Input;
using WindowsDev.Business.Services.Logger;
using WindowsDev.Business.Services.ProjectService.Interfaces;
using WindowsDev.Business.Services.TaskService.Interfaces;
using WindowsDev.Domain.TasksModels;
using WindowsDev.Infrastructure;
using WindowsDev.ViewModels.Interfaces;

namespace WindowsDev.ViewModels.Tasks.Dialog
{
    public class EditTaskViewModel : TaskDialogViewModelBase, IInitializableAsync, IProjectDialogCreator
    {
        private readonly ITaskService _taskService;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly ILogger<EditTaskViewModel> _logger;

        private TasksInfo CurrentTask { get; set; } = null!;

        public EditTaskViewModel(ITaskService taskService, IDialogCoordinator dialogCoordinator,
            ILogger<EditTaskViewModel> logger)
        {
            _taskService = taskService;
            _dialogCoordinator = dialogCoordinator;
            _logger = logger;

            EditTaskCommand = new AsyncRelayCommand(EditTask);
        }

        // Commands
        public ICommand EditTaskCommand { get; }

        // Events
        public event Func<Task>? CloseRequested;
        public event Func<Task>? Completed;

        // Init
        public async Task InitializationAsync(params object[] parameter)
        {
            CurrentTask = parameter.OfType<TasksInfo>().FirstOrDefault()
                ?? throw new ArgumentNullException();

                SetEditDialog();
        }

        // Commands logic
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

                    Completed?.Invoke();
                    await Close();
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
                _logger.LogError(ex, "Failed to edit task {taskId}", CurrentTask.Id);
                await _dialogCoordinator.ShowMessageAsync(this,
                    "Error",
                    "Failed to edit task",
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
                await CloseRequested.Invoke();

            IsEditMode = false;
        }
    }
}