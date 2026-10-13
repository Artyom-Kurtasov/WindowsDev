using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Windows.Input;
using WindowsDev.ApiClient.TaskClient;
using WindowsDev.Application.Common.Localization;
using WindowsDev.Command;
using WindowsDev.Contracts.Api.Tasks;
using WindowsDev.Contracts.Tasks;
using WindowsDev.Domain.Messages;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;
using WindowsDev.Infrastructure.Logging;
using WindowsDev.Services.Dialogs;

namespace WindowsDev.ViewModels.Tasks.Dialogs;

internal class EditTaskViewModel : TaskDialogViewModelBase, IDialogViewModel
{
    private readonly ITaskApiClient _apiClient;
    private readonly ILogger<EditTaskViewModel> _logger;
    private readonly GetTasksResponse _currentTask;

    public EditTaskViewModel(
        GetTasksResponse currentTask,
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

            var taskToUpdate = new UpdateTaskRequest
            {
                Name = _currentTask.Name,
                Description = _currentTask.Description,
                Deadline = _currentTask.Deadline,
                Priority = _currentTask.Priority,
                Status = _currentTask.Status,
                Progress = _currentTask.Progress,
            };

            Debug.WriteLine(_currentTask.Id);
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