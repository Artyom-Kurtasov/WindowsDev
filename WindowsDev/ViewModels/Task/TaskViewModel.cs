using System.Collections.ObjectModel;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using WindowsDev.Business.Services.Localization.Interfaces;
using WindowsDev.Business.Services.TaskService.Attachment.Interfaces;
using WindowsDev.Business.Services.TaskService.Comment.Interfaces;
using WindowsDev.Dialogs.Interfaces;
using WindowsDev.Domain;
using WindowsDev.Domain.DialogsMessages.Errors;
using WindowsDev.Domain.ProjectsModels;
using WindowsDev.Domain.TasksModels;
using WindowsDev.Domain.TasksModels.Enums;
using WindowsDev.Infrastructure;
using WindowsDev.Infrastructure.Logging;
using WindowsDev.NavigationManager.Interfaces;
using WindowsDev.ViewModels.Interfaces;
using WindowsDev.ViewModels.Projects;
using WindowsDev.ViewModels.Tasks.Dialog;
using WindowsDev.Views.Tasks;

namespace WindowsDev.ViewModels.Tasks
{
    public class TaskViewModel : LocalizedViewModelBase, IRefreshableViewModel, IDisposable
    {
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly IAttacmentService _attacmentService;
        private readonly ICommentService _commentService;
        private readonly ILogger<TaskViewModel> _logger;

        private bool disposedValue;

        public TaskViewModel(
            ProjectsInfo project,
            TasksInfo currentTask,
            ICommentService commentService,
            IDialogService dialogService,
            INavigationService navigationService,
            IAttacmentService attacmentService,
            ILogger<TaskViewModel> logger,
            IDialogCoordinator dialogCoordinator,
            ILanguageChanger languageChanger
        )
            : base(languageChanger)
        {
            ArgumentNullException.ThrowIfNull(project);
            ArgumentNullException.ThrowIfNull(currentTask);

            Project = project;
            _currentTask = currentTask;

            _commentService = commentService;
            _dialogService = dialogService;
            _navigationService = navigationService;
            _attacmentService = attacmentService;
            _logger = logger;
            _dialogCoordinator = dialogCoordinator;

            AddCommentCommand = new AsyncRelayCommand(AddComment);
            EditTaskCommand = new AsyncRelayCommand(EditTask);
            SwitchToProjectCommand = new RelayCommand(SwitchToProject);
            AddAttachmentCommand = new AsyncRelayCommand(AddAttachment);
            OpenAttachmentCommand = new AsyncRelayCommandT<TaskAttachment>(OpenAttachment);

            _ = LoadDetailsAsync();
        }

        public ICommand SwitchToProjectCommand { get; }
        public ICommand EditTaskCommand { get; }
        public ICommand AddCommentCommand { get; }
        public ICommand AddAttachmentCommand { get; }
        public ICommand OpenAttachmentCommand { get; }

        public ProjectsInfo? Project { get; private set; }

        private TasksInfo? _currentTask;
        public TasksInfo? CurrentTask
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

        public ObservableCollection<Comments>? Comments { get; private set; } = new();

        public ObservableCollection<TaskAttachment>? Attachments { get; private set; } = new();

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

        public Domain.TasksModels.Enums.TaskStatus Status
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
            get => CurrentTask!.DeadLine;
            set
            {
                if (CurrentTask!.DeadLine == value)
                    return;

                CurrentTask.DeadLine = value;
                OnPropertyChanged(nameof(DeadLine));
            }
        }

        public DateTime CreatedAt => CurrentTask!.CreatedAt;

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

                var comments = await _commentService.GetComments(CurrentTask!.Id);

                foreach (var comment in comments)
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

                var attachments = await _attacmentService.GetAttachmentsAsync(CurrentTask!.Id);

                foreach (var attachment in attachments)
                    Attachments.Add(attachment);
            }
            catch (Exception ex)
            {
                AttachmentLogs.AttachmentLoadFailed(_logger, CurrentTask!.Id.ToString(), ex);

                await ShowErrorAsync(TaskErrors.LoadAttachmentsFailed);
            }
        }

        private async Task OpenAttachment(TaskAttachment attachment)
        {
            if (attachment == null)
                return;

            try
            {
                await _attacmentService.OpenFile(attachment);
            }
            catch (Exception ex)
            {
                AttachmentLogs.AttachmentOpenFailed(
                    _logger,
                    attachment.Id.ToString(),
                    attachment.FilePath,
                    ex
                );

                await ShowErrorAsync(TaskErrors.OpenAttachmentFailed);
            }
        }

        private async Task AddAttachment()
        {
            try
            {
                var result = await _attacmentService.AddFile(CurrentTask!.Id);

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
                var result = await _commentService.AddComment(CurrentTask!.Id, NewComment);

                if (result.IsSuccess)
                {
                    Comments!.Add(result.Value);
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
            await _dialogService.ShowDialogAsync<TaskDialogView, EditTaskViewModel>(
                this,
                CurrentTask!
            );
        }

        private void SwitchToProject()
        {
            _navigationService.NavigateTo<ProjectViewModel>(Project!);
            Dispose();
        }

        private async Task ShowErrorAsync(string error)
        {
            await _dialogCoordinator.ShowMessageAsync(
                this,
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
}
