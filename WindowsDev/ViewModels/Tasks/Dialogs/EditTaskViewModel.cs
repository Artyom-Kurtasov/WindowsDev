using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using System.Windows.Input;
using WindowsDev.ApiClient.TaskClient;
using WindowsDev.Application.Common.Localization;
using WindowsDev.Command;
using WindowsDev.Contracts.UI.Tasks;
using WindowsDev.Domain.Messages;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;
using WindowsDev.Logging;
using WindowsDev.Services.Dialogs;

namespace WindowsDev.ViewModels.Tasks.Dialogs;

internal class EditTaskViewModel : TaskDialogViewModelBase, IDialogViewModel
{
    private readonly ITaskApiClient _apiClient;
    private readonly ILogger<EditTaskViewModel> _logger;
    private readonly TaskInfo _currentTask;

    public EditTaskViewModel(
        TaskInfo currentTask,
        ITaskApiClient apiClient,
        IDialogService dialogService,
        ILogger<EditTaskViewModel> logger,
        ILanguageChanger languageChanger
    )
        : base(languageChanger, dialogService)
    {
        _currentTask = currentTask;
        _logger = logger;
        _apiClient = apiClient;

        SubmitCommand = new AsyncRelayCommand(EditTaskAsync);
        CancelCommand = new AsyncRelayCommand(CloseAsync);

        SetEditDialog();
    }

    public override ICommand SubmitCommand { get; }
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

            var taskToUpdate = new UpdateTaskForm(
                Progress: _currentTask.Progress,
                Name: _currentTask.Name,
                Description: _currentTask.Description,
                Priority: _currentTask.Priority,
                Status: _currentTask.Status,
                Deadline: _currentTask.Deadline
            );

            await _apiClient.UpdateAsync(_currentTask.Id, taskToUpdate);

            if (Completed != null)
                await Completed.Invoke();

            await CloseAsync();
        }
        catch (Exception ex)
        {
            TaskLogs.TaskUpdateFailed(_logger, _currentTask.Id, ex);

            await _dialogService.ShowMessageAsync(
                Translate(DialogTitles.Error),
                Translate(CommonErrors.UnexpectedError),
                MessageDialogStyle.Affirmative
            );
        }
    }

    private void UpdateTask()
    {
        _currentTask.Progress = Progress;
        _currentTask.Name = Name;
        _currentTask.Description = Description;
        _currentTask.Priority = Priority;
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
