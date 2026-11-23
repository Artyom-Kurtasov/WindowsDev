using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using WindowsDev.ApiClient.AttachmentClient;
using WindowsDev.ApiClient.CommentClient;
using WindowsDev.Application.Common.Localization;
using WindowsDev.Command;
using WindowsDev.Contracts.UI.Attachments;
using WindowsDev.Contracts.UI.Comments;
using WindowsDev.Contracts.UI.Projects;
using WindowsDev.Contracts.UI.Tasks;
using WindowsDev.Domain.Enums;
using WindowsDev.Domain.Messages;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;
using WindowsDev.Logging;
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
    private readonly HashSet<string> _openedAttachments = new(StringComparer.OrdinalIgnoreCase);
    private readonly AsyncRelayCommandT<string> _openAttachmentCommand;

    private bool disposedValue;

    public TaskViewModel(
        ProjectInfo project,
        TaskInfo currentTask,
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
        _openAttachmentCommand = new AsyncRelayCommandT<string>(OpenAttachment, CanOpenAttachment);
        OpenAttachmentCommand = _openAttachmentCommand;

        _ = LoadDetailsAsync();
    }

    public ICommand SwitchToProjectCommand { get; }
    public ICommand EditTaskCommand { get; }
    public ICommand AddCommentCommand { get; }
    public ICommand AddAttachmentCommand { get; }
    public ICommand OpenAttachmentCommand { get; }

    public ProjectInfo? Project { get; private set; }

    private TaskInfo? _currentTask;
    public TaskInfo? CurrentTask
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

    public ObservableCollection<CommentInfo>? Comments { get; private set; } = new();

    public ObservableCollection<FileItem>? Attachments { get; private set; } = new();

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

            var result = await _attachmentApiClient.GetFilesAsync(CancellationToken.None);

            if (!result.IsSuccess || result.Value is null)
            {
                await ShowErrorAsync(TaskErrors.LoadAttachmentsFailed);
                return;
            }

            foreach (var item in result.Value)
                Attachments.Add(item);
        }
        catch (Exception ex)
        {
            AttachmentLogs.AttachmentLoadFailed(_logger, CurrentTask!.Id.ToString(), ex);

            await ShowErrorAsync(TaskErrors.LoadAttachmentsFailed);
        }
    }

    private bool _isOpeningAttachment;

    private bool CanOpenAttachment(string fileName)
    {
        return !_isOpeningAttachment && !_openedAttachments.Contains(fileName);
    }

    private async Task OpenAttachment(string fileName)
    {
        if (!CanOpenAttachment(fileName))
            return;

        _isOpeningAttachment = true;
        _openAttachmentCommand.RaiseCanExecuteChanged();
        try
        {
            var result = await _attachmentApiClient.DownloadFileAsync(
                fileName,
                CancellationToken.None
            );

            if (!result.IsSuccess || result.Value is null)
            {
                await ShowErrorAsync(result.Error);
                return;
            }

            var dto = result.Value;
            var tempPath = Path.Combine(Path.GetTempPath(), dto.FileName);

            await using (var fileStream = File.Create(tempPath))
            {
                await dto.Content.CopyToAsync(fileStream);
            }
            await dto.Content.DisposeAsync();

            var process = Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });
            if (process is null)
                throw new InvalidOperationException("The attachment process could not be started.");

            _openedAttachments.Add(fileName);
        }
        catch (Exception ex)
        {
            AttachmentLogs.AttachmentLoadFailed(_logger, fileName, ex);
            await ShowErrorAsync(TaskErrors.OpenAttachmentFailed);
        }
        finally
        {
            _isOpeningAttachment = false;
            _openAttachmentCommand.RaiseCanExecuteChanged();
        }
    }

    private async Task AddAttachment()
    {
        try
        {
            var file = PickFile();

            if (file != null)
            {
                await using var stream = File.OpenRead(file);

                var result = await _attachmentApiClient.UploadFileAsync(
                    new UploadFileForm(
                        Path.GetFileName(file),
                        "application/octet-stream",
                        stream
                    ),
                    CancellationToken.None
                );

                if (result.IsSuccess)
                {
                    await LoadAttachmentsAsync();
                    return;
                }

                AttachmentLogs.AttachmentUploadFailed(
                    _logger,
                    Path.GetFileName(file),
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
                new AddCommentForm(CurrentTask!.Id, NewComment),
                CancellationToken.None
            );

            if (result.IsSuccess && result.Value is not null)
            {
                Comments!.Add(result.Value);
                NewComment = string.Empty;
            }

            await LoadCommentsAsync();
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
        RefreshTask();
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
                _openedAttachments.Clear();
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