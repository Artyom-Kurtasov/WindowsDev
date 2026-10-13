using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Windows.Input;
using WindowsDev.ApiClient.AttachmentClient;
using WindowsDev.ApiClient.CommentClient;
using WindowsDev.Application.Common.Localization;
using WindowsDev.Command;
using WindowsDev.Contracts.Api.Attachments;
using WindowsDev.Contracts.Api.Comments;
using WindowsDev.Contracts.Api.Projects;
using WindowsDev.Contracts.Api.Tasks;
using WindowsDev.Contracts.UI;
using WindowsDev.Domain.Enums;
using WindowsDev.Domain.Messages;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;
using WindowsDev.Infrastructure.Logging;
using WindowsDev.Services.Dialogs;
using WindowsDev.Services.Navigation;
using WindowsDev.ViewModels.Interfaces;
using WindowsDev.ViewModels.Project;
using WindowsDev.ViewModels.Tasks.Dialogs;
using WindowsDev.Views.Tasks;

namespace WindowsDev.ViewModels.Tasks;

internal class TaskViewModel : LocalizedViewModelBase, IRefreshableViewModel, IDisposable
{
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly IAttachmentApiClient _attachmentApiClient;
    private readonly ICommentApiClient _commentApiClient;
    private readonly ILogger<TaskViewModel> _logger;

    private bool disposedValue;

    public TaskViewModel(
        GetProjectsResponse project,
        GetTasksResponse currentTask,
        ICommentApiClient commentApiClient,
        IDialogService dialogService,
        INavigationService navigationService,
        IAttachmentApiClient attachmentApiClient,
        ILogger<TaskViewModel> logger,
        ILanguageChanger languageChanger
    )
        : base(languageChanger)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(currentTask);

        Project = project;
        _currentTask = currentTask;

        _commentApiClient = commentApiClient;
        _dialogService = dialogService;
        _navigationService = navigationService;
        _attachmentApiClient = attachmentApiClient;
        _logger = logger;

        AddCommentCommand = new AsyncRelayCommand(AddComment);
        EditTaskCommand = new AsyncRelayCommand(EditTask);
        SwitchToProjectCommand = new RelayCommand(SwitchToProject);
        AddAttachmentCommand = new AsyncRelayCommand(AddAttachment);
        OpenAttachmentCommand = new AsyncRelayCommandT<string>(OpenAttachment);

        _ = LoadDetailsAsync();
    }

    public ICommand SwitchToProjectCommand { get; }
    public ICommand EditTaskCommand { get; }
    public ICommand AddCommentCommand { get; }
    public ICommand AddAttachmentCommand { get; }
    public ICommand OpenAttachmentCommand { get; }

    public GetProjectsResponse? Project { get; private set; }

    private GetTasksResponse? _currentTask;
    public GetTasksResponse? CurrentTask
    {
        get => _currentTask;
        set
        {
            if (_currentTask == value)
                return;

            _currentTask = value;
            RefreshTask();
        }
    }

    public ObservableCollection<GetCommentResponse>? Comments { get; private set; } = new();

    public ObservableCollection<AttachmentInfo>? Attachments { get; private set; } = new();

    private string? _newComment;
    public string? NewComment
    {
        get => _newComment;
        set
        {
            _newComment = value;
            OnPropertyChanged(nameof(NewComment));
        }
    }

    public string Name
    {
        get => CurrentTask!.Name;
        set
        {
            if (CurrentTask!.Name == value)
                return;

            CurrentTask.Name = value;
            OnPropertyChanged(nameof(Name));
        }
    }

    public string? Description
    {
        get => CurrentTask!.Description;
        set
        {
            if (CurrentTask!.Description == value)
                return;

            CurrentTask.Description = value;
            OnPropertyChanged(nameof(Description));
        }
    }

    public TaskPriority Priority
    {
        get => CurrentTask!.Priority;
        set
        {
            if (CurrentTask!.Priority == value)
                return;

            CurrentTask.Priority = value;
            OnPropertyChanged(nameof(Priority));
        }
    }

    public int Progress
    {
        get => CurrentTask!.Progress;
        set
        {
            if (CurrentTask!.Progress == value)
                return;

            CurrentTask.Progress = value;
            OnPropertyChanged(nameof(Progress));
        }
    }

    public Domain.Enums.TaskStatus Status
    {
        get => CurrentTask!.Status;
        set
        {
            if (CurrentTask!.Status == value)
                return;

            CurrentTask.Status = value;
            OnPropertyChanged(nameof(Status));
        }
    }

    public DateTime DeadLine
    {
        get => CurrentTask!.Deadline.ToLocalTime();
        set
        {
            if (CurrentTask!.Deadline == value)
                return;

            CurrentTask.Deadline = value;
            OnPropertyChanged(nameof(DeadLine));
        }
    }

    public DateTime CreatedAt => CurrentTask!.CreatedAt;

    public string DateRange
    {
        get => $"{CreatedAt:dd.MM.yyyy} - {DeadLine:dd.MM.yyyy}";
        set { OnPropertyChanged(nameof(DateRange)); }
    }

    public async Task RefreshAsync()
    {
        await LoadDetailsAsync();
        RefreshTask();
    }

    private async Task LoadDetailsAsync()
    {
        await LoadCommentsAsync();
        await LoadAttachmentsAsync();
    }

    private async Task LoadCommentsAsync()
    {
        try
        {
            Comments!.Clear();

            var result = await _commentApiClient.GetCommentsAsync(
                CurrentTask!.Id,
                CancellationToken.None
            );

            if (!result.IsSuccess || result.Value is null)
            {
                await ShowErrorAsync(TaskErrors.LoadCommentsFailed);
                return;
            }

            foreach (var comment in result.Value)
                Comments.Add(comment);
        }
        catch (Exception ex)
        {
            CommentLogs.CommentLoadFailed(_logger, CurrentTask!.Id.ToString(), ex);

            await ShowErrorAsync(TaskErrors.LoadCommentsFailed);
        }
    }

    private async Task LoadAttachmentsAsync()
    {
        try
        {
            Attachments!.Clear();

            var attachments = await _attachmentApiClient.GetAttachmentsAsync(CurrentTask.Id);

            foreach (var item in attachments.Value)
            {
                Attachments.Add(new(item.FileExtension,
                    item.FilePath,
                    item.FileName,
                    item.FileSize,
                    item.TaskId));
            }
        }
        catch (Exception ex)
        {
            AttachmentLogs.AttachmentLoadFailed(_logger, CurrentTask!.Id.ToString(), ex);

            await ShowErrorAsync(TaskErrors.LoadAttachmentsFailed);
        }
    }

    private async Task OpenAttachment(string filePath)
    {
        try
        {
            await _attachmentApiClient.OpenAttachmentAsync(filePath);
        }
        catch (Exception ex)
        {
            //AttachmentLogs.AttachmentOpenFailed(
            //    _logger,
            //    attachment.Id.ToString(),
            //    attachment.FilePath,
            //    ex
            //);

            await ShowErrorAsync(TaskErrors.OpenAttachmentFailed);
        }
    }

    private async Task AddAttachment()
    {
        try
        {
            var file = PickFile();

            if (file != null)
            {
                var result = await _attachmentApiClient.AddFileAsync(new(file,
                    CurrentTask.Id));

                if (result.IsSuccess)
                {
                    Attachments!.Add(result.Value);
                    return;
                }

                AttachmentLogs.AttachmentUploadFailed(
                    _logger,
                    "unknown",
                    0,
                    new InvalidOperationException(result.Error)
                );

                await ShowErrorAsync(result.Error);
            }
        }
        catch (Exception ex)
        {
            AttachmentLogs.AttachmentUploadFailed(_logger, "unknown", 0, ex);

            await ShowErrorAsync(CommonErrors.UnexpectedError);
        }
    }

    private async Task AddComment()
    {
        if (string.IsNullOrWhiteSpace(NewComment))
            return;

        try
        {
            var result = await _commentApiClient.AddCommentAsync(
                new AddCommentRequest { TaskId = CurrentTask!.Id, Text = NewComment },
                CancellationToken.None
            );

            if (result.IsSuccess && result.Value is not null)
            {
                Comments!.Add(
                    new GetCommentResponse
                    {
                        Id = result.Value.Id,
                        Text = result.Value.Text,
                        CreatedAt = result.Value.CreatedAt,
                        Author = result.Value.Author,
                        TaskId = result.Value.TaskId,
                    }
                );
                NewComment = string.Empty;
            }
        }
        catch (Exception ex)
        {
            CommentLogs.CommentCreationFailed(_logger, CurrentTask!.Id.ToString(), ex);

            await ShowErrorAsync(CommonErrors.UnexpectedError);
        }
    }

    private async Task EditTask()
    {
        await _dialogService.ShowDialogAsync<TaskDialogView, EditTaskViewModel>(CurrentTask!);
    }

    private void SwitchToProject()
    {
        _navigationService.NavigateTo<ProjectViewModel>(Project!);
        Dispose();
    }

    private async Task ShowErrorAsync(string error)
    {
        await _dialogService.ShowMessageAsync(
            Translate(DialogTitles.Error),
            Translate(error),
            MessageDialogStyle.Affirmative
        );
    }

    public void RefreshTask()
    {
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(Description));
        OnPropertyChanged(nameof(Priority));
        OnPropertyChanged(nameof(Progress));
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(DeadLine));
        OnPropertyChanged(nameof(CreatedAt));
        OnPropertyChanged(nameof(DateRange));
    }

    private string? PickFile()
    {
        var dialog = new OpenFileDialog();
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                Comments = null;
                Attachments = null;
                _currentTask = null;
                Project = null;
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
