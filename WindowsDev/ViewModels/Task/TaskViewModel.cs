using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
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
    public class TaskViewModel : ViewModelBase, IRefreshableViewModel
    {
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly IAttacmentService _attacmentService;
        private readonly ICommentService _commentService;
        private readonly ILogger<TaskViewModel> _logger;

        private TasksInfo _currentTask;
        private string? _newComment;

        public TaskViewModel(ProjectsInfo project,
            TasksInfo currentTask,
            ICommentService commentService,
            IDialogService dialogService,
            INavigationService navigationService,
            IAttacmentService attacmentService,
            ILogger<TaskViewModel> logger,
            IDialogCoordinator dialogCoordinator)
        {
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
            SwitchToProjectCommand = new RelayCommand(SwitchToProject, CanSwitchToProject);
            AddAttachmentCommand = new AsyncRelayCommand(AddAttachment);
            OpenAttachmentCommand = new AsyncRelayCommandT<TaskAttachment>(OpenAttachment);

            _ = LoadDetailsAsync();
        }

        public ICommand SwitchToProjectCommand { get; }
        public ICommand EditTaskCommand { get; }
        public ICommand AddCommentCommand { get; }
        public ICommand AddAttachmentCommand { get; }
        public ICommand OpenAttachmentCommand { get; }

        public ProjectsInfo Project { get; }

        public TasksInfo CurrentTask
        {
            get => _currentTask;
            set
            {
                _currentTask = value;

                OnPropertyChanged(nameof(CurrentTask));
                OnPropertyChanged(nameof(Name));
                OnPropertyChanged(nameof(Description));
                OnPropertyChanged(nameof(Priority));
                OnPropertyChanged(nameof(Progress));
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(DeadLine));
                OnPropertyChanged(nameof(CreatedAt));
            }
        }

        private ObservableCollection<Comments> _comments = new();

        public ObservableCollection<Comments> Comments
        {
            get => _comments;
            set
            {
                _comments = value;
                OnPropertyChanged(nameof(Comments));
            }
        }

        private ObservableCollection<TaskAttachment> _attachments = new();

        public ObservableCollection<TaskAttachment> Attachments
        {
            get => _attachments;
            set
            {
                _attachments = value;
                OnPropertyChanged(nameof(Attachments));
            }
        }

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
            get => CurrentTask.Name;
            set
            {
                if (CurrentTask.Name == value) return;

                CurrentTask.Name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public string? Description
        {
            get => CurrentTask.Description;
            set
            {
                if (CurrentTask.Description == value) return;

                CurrentTask.Description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        public TaskPriority Priority
        {
            get => CurrentTask.Priority;
            set
            {
                if (CurrentTask.Priority == value) return;

                CurrentTask.Priority = value;
                OnPropertyChanged(nameof(Priority));
            }
        }

        public int Progress
        {
            get => CurrentTask.Progress;
            set
            {
                if (CurrentTask.Progress == value) return;

                CurrentTask.Progress = value;
                OnPropertyChanged(nameof(Progress));
            }
        }

        public Domain.TasksModels.Enums.TaskStatus Status
        {
            get => CurrentTask.Status;
            set
            {
                if (CurrentTask.Status == value) return;

                CurrentTask.Status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public DateTime DeadLine
        {
            get => CurrentTask.DeadLine;
            set
            {
                if (CurrentTask.DeadLine == value) return;

                CurrentTask.DeadLine = value;
                OnPropertyChanged(nameof(DeadLine));
            }
        }

        public DateTime CreatedAt => CurrentTask.CreatedAt;

        public Task RefreshAsync()
        {
            RefreshTask();
            return Task.CompletedTask;
        }

        private async Task LoadDetailsAsync()
        {
            var taskId = CurrentTask.Id.ToString();

            try
            {
                Comments = new ObservableCollection<Comments>(
                    await _commentService.GetComments(CurrentTask.Id));
            }
            catch (Exception ex)
            {
                CommentLogs.CommentLoadFailed(_logger, taskId, ex);
                await _dialogCoordinator.ShowMessageAsync(this,
                    Translate(DialogTitles.Error),
                    Translate(CommentErrors.LoadFailed),
                    MessageDialogStyle.Affirmative);

                Comments = new ObservableCollection<Comments>();
            }

            try
            {
                Attachments = new ObservableCollection<TaskAttachment>(
                    await _attacmentService.GetAttachmentsAsync(CurrentTask.Id));
            }
            catch (Exception ex)
            {
                AttachmentLogs.AttachmentLoadFailed(_logger, taskId, ex);
                await _dialogCoordinator.ShowMessageAsync(this,
                    Translate(DialogTitles.Error),
                    Translate(AttachmentErrors.LoadFailed),
                    MessageDialogStyle.Affirmative);

                Attachments = new ObservableCollection<TaskAttachment>();
            }
        }

        private async Task OpenAttachment(TaskAttachment taskAttachment)
        {
            if (taskAttachment == null || string.IsNullOrWhiteSpace(taskAttachment.FilePath))
                return;

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = taskAttachment.FilePath,
                    UseShellExecute = true
                });
            }
            catch (FileNotFoundException ex)
            {
                AttachmentLogs.AttachmentNotFound(_logger, taskAttachment.Id.ToString());
                await _dialogCoordinator.ShowMessageAsync(this,
                    Translate(DialogTitles.Error),
                    Translate(TaskErrors.FileNotFound),
                    MessageDialogStyle.Affirmative);
            }
            catch (Exception ex)
            {
                AttachmentLogs.AttachmentDownloadFailed(
                    _logger,
                    taskAttachment.Id.ToString(),
                    taskAttachment.FilePath,
                    ex);
                await _dialogCoordinator.ShowMessageAsync(this,
                    Translate(DialogTitles.Error),
                    Translate(AttachmentErrors.CannotOpenFile),
                    MessageDialogStyle.Affirmative);
            }
        }

        private async Task AddAttachment()
        {
            try
            {
                var dialog = new OpenFileDialog();

                if (dialog.ShowDialog() == true)
                {
                    var result = await _attacmentService.AddFile(dialog.FileName, CurrentTask.Id);

                    if (result.IsSuccess)
                    {
                        Attachments?.Add(result.Value);
                    }
                    else
                    {
                        AttachmentLogs.AttachmentUploadFailed(
                            _logger,
                            dialog.FileName,
                            new FileInfo(dialog.FileName).Length,
                            new InvalidOperationException(result.Error));
                        await _dialogCoordinator.ShowMessageAsync(this,
                            Translate(DialogTitles.Error),
                            Translate(result.Error),
                            MessageDialogStyle.Affirmative);
                    }
                }
            }
            catch (Exception ex)
            {
                AttachmentLogs.AttachmentUploadFailed(_logger, "unknown", 0, ex);
                await _dialogCoordinator.ShowMessageAsync(this,
                    Translate(DialogTitles.Error),
                    Translate(CommonErrors.UnexpectedError),
                    MessageDialogStyle.Affirmative);
            }
        }

        private async Task AddComment()
        {
            if (string.IsNullOrWhiteSpace(_newComment))
                return;

            var taskId = CurrentTask.Id.ToString();

            try
            {
                var result = await _commentService.AddComment(CurrentTask.Id, _newComment);

                if (result.IsSuccess)
                {
                    Comments?.Add(result.Value);
                    NewComment = string.Empty;
                }
                else
                {
                    CommentLogs.CommentCreationFailed(
                        _logger,
                        taskId,
                        new InvalidOperationException(result.Error));
                    await _dialogCoordinator.ShowMessageAsync(this,
                        Translate(DialogTitles.Error),
                        Translate(result.Error),
                        MessageDialogStyle.Affirmative);
                }
            }
            catch (Exception ex)
            {
                CommentLogs.CommentCreationFailed(_logger, taskId, ex);
                await _dialogCoordinator.ShowMessageAsync(this,
                    Translate(DialogTitles.Error),
                    Translate(CommonErrors.UnexpectedError),
                    MessageDialogStyle.Affirmative);
            }
        }

        private async Task EditTask() =>
            await _dialogService.ShowDialogAsync<
                TaskDialogView,
                EditTaskViewModel>(this, CurrentTask);

        private void SwitchToProject() =>
            _navigationService.NavigateTo<ProjectViewModel>(Project);

        private bool CanSwitchToProject() => true;

        public void RefreshTask()
        {
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Description));
            OnPropertyChanged(nameof(Priority));
            OnPropertyChanged(nameof(Progress));
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(DeadLine));
        }
    }
}
