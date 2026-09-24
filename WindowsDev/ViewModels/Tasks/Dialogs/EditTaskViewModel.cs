using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using System.Windows.Input;
using WindowsDev.Application.Common.Utils.Localization;
using WindowsDev.Application.Tasks;
using WindowsDev.Command;
using WindowsDev.Domain.Entities;
using WindowsDev.Domain.Messages;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;
using WindowsDev.Infrastructure.Logging;
using WindowsDev.Services.Dialogs;

namespace WindowsDev.ViewModels.Tasks.Dialogs;

internal class EditTaskViewModel : TaskDialogViewModelBase, IDialogViewModel
{
    private readonly ITaskService _taskService;
    private readonly ILogger<EditTaskViewModel> _logger;
    private readonly TasksInfo _currentTask;

    public EditTaskViewModel(
        TasksInfo currentTask,
        ITaskService taskService,
        IDialogCoordinator dialogCoordinator,
        ILogger<EditTaskViewModel> logger,
        ILanguageChanger languageChanger
    )
        : base(languageChanger, dialogCoordinator)
    {
        _currentTask = currentTask;
        _taskService = taskService;
        _logger = logger;

        EditTaskCommand = new AsyncRelayCommand(EditTaskAsync);
        CancelCommand = new AsyncRelayCommand(CloseAsync);

        SetEditDialog();
    }

    public ICommand EditTaskCommand { get; }
    public ICommand CancelCommand { get; }

    public event Func<Task>? CloseRequested;
    public event Func<Task>? Completed;

    private async Task EditTaskAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            await ShowWarningAsync();
            return;
        }

        try
        {
            UpdateTask();

            await _taskService.UpdateAsync(_currentTask);

            if (Completed != null)
                await Completed.Invoke();

            await CloseAsync();
        }
        catch (Exception ex)
        {
            TaskLogs.TaskUpdateFailed(_logger, _currentTask.Id, ex);

            await _dialogCoordinator.ShowMessageAsync(
                this,
                Translate(DialogTitles.Error),
                Translate(CommonErrors.UnexpectedError),
                MessageDialogStyle.Affirmative
            );
        }
    }

    private void UpdateTask()
    {
        _currentTask.Name = Name;
        _currentTask.Description = Description;
        _currentTask.Priority = Priority;
        _currentTask.Progress = Progress;
        _currentTask.Status = Status;
        _currentTask.Deadline = DeadLine.ToUniversalTime();
    }

    private void SetEditDialog()
    {
        Name = _currentTask.Name;
        Description = _currentTask.Description ?? string.Empty;
        Priority = _currentTask.Priority;
        Progress = _currentTask.Progress;
        Status = _currentTask.Status;
        DeadLine = _currentTask.Deadline.ToLocalTime();

        IsEditMode = true;
    }

    private async Task CloseAsync()
    {
        if (CloseRequested != null)
            await CloseRequested.Invoke();

        IsEditMode = false;
    }
}