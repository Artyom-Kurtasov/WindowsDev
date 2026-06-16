using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using WindowsDev.Business.Services.TaskService.Attachment.Interfaces;
using WindowsDev.Business.Services.TaskService.Comment.Interfaces;
using WindowsDev.Commands.NavigationManager.Interfaces;
using WindowsDev.Dialogs.Interfaces;
using WindowsDev.Domain.ProjectsModels;
using WindowsDev.Domain.TasksModels;
using WindowsDev.Domain.TasksModels.Enums;
using WindowsDev.Infrastructure;
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

            // Fire-and-forget: errors are handled and logged inside LoadDetailsAsync
            _ = LoadDetailsAsync();
        }

        public ICommand SwitchToProjectCommand { get; }
        public ICommand EditTaskCommand { get; }
        public ICommand AddCommentCommand { get; }
        public ICommand AddAttachmentCommand { get; }
        public ICommand OpenAttachmentCommand { get; }

        public ProjectsInfo Project { get; }

        // Proxy properties that delegate to CurrentTask.
        // CurrentTask is mutated directly — EditTaskViewModel and RefreshAsync
        // both modify it, then we notify the UI about changed fields
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

        // Delegate properties — read/write directly to CurrentTask.
        // No separate backing fields needed, but we must manually notify
        // because CurrentTask doesn't implement INotifyPropertyChanged
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

        // RefreshAsync is called by DialogService after EditTaskViewModel completes.
        // We only need to re-notify the UI — CurrentTask was already mutated by the dialog
        public Task RefreshAsync()
        {
            RefreshTask();
            return Task.CompletedTask;
        }

        private async Task LoadDetailsAsync()
        {
            // Comments and attachments are loaded independently —
            // failure of one doesn't prevent loading the other
            try
            {
                Comments = new ObservableCollection<Comments>(
                    await _commentService.GetComments(CurrentTask.Id));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load comments for task {TaskId}", CurrentTask.Id);
                Comments = new ObservableCollection<Comments>();

                await _dialogCoordinator.ShowMessageAsync(this,
                    Translate("Error_Title"),
                    Translate("Error_LoadComments"),
                    MessageDialogStyle.Affirmative);
            }

            try
            {
                Attachments = new ObservableCollection<TaskAttachment>(
                    await _attacmentService.GetAttachmentsAsync(CurrentTask.Id));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load attachments for task {TaskId}", CurrentTask.Id);
                Attachments = new ObservableCollection<TaskAttachment>();

                await _dialogCoordinator.ShowMessageAsync(this,
                    Translate("Error_Title"),
                    Translate("Error_LoadAttachments"),
                    MessageDialogStyle.Affirmative);
            }
        }

        private async Task OpenAttachment(TaskAttachment taskAttachment)
        {
            if (taskAttachment == null || string.IsNullOrWhiteSpace(taskAttachment.FilePath))
                return;

            try
            {
                // UseShellExecute = true delegates to the OS default handler
                // for the file type (PDF reader, image viewer, etc.)
                Process.Start(new ProcessStartInfo
                {
                    FileName = taskAttachment.FilePath,
                    UseShellExecute = true
                });
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogError(ex, "Attachment file not found: {FilePath}", taskAttachment.FilePath);
                await _dialogCoordinator.ShowMessageAsync(this,
                    Translate("Error_Title"),
                    Translate("Error_FileNotFound"),
                    MessageDialogStyle.Affirmative);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to open attachment: {FilePath}", taskAttachment.FilePath);
                await _dialogCoordinator.ShowMessageAsync(this,
                    Translate("Error_Title"),
                    Translate("Error_File"),
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
                    var attachment =
                        await _attacmentService.AddFile(dialog.FileName, CurrentTask.Id);

                    if (attachment != null)
                        Attachments?.Add(attachment);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add attachment for task {TaskId}", CurrentTask.Id);
                await _dialogCoordinator.ShowMessageAsync(this,
                    Translate("Error_Title"),
                    Translate("Error_AddAttachment"),
                    MessageDialogStyle.Affirmative);
            }
        }

        private async Task AddComment()
        {
            if (string.IsNullOrWhiteSpace(_newComment))
                return;

            try
            {
                var comment = await _commentService.AddComment(CurrentTask.Id, _newComment);

                Comments?.Add(comment);
                NewComment = string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add comment for task {TaskId}", CurrentTask.Id);
                await _dialogCoordinator.ShowMessageAsync(this,
                    Translate("Error_Title"),
                    Translate("Error_AddComment"),
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

        // Called after EditTaskViewModel mutates CurrentTask.
        // We only need to re-notify the UI — the data is already updated in-place
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