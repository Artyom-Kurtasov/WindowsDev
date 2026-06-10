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
    public class TaskViewModel : ViewModelBase, IInitializableAsync
    {
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly IAttacmentService _attacmentService;
        private readonly ICommentService _commentService; 
        private readonly ILogger<TaskViewModel> _logger;

        private TasksInfo _currentTask = null!;
        private string? _newComment;

        public TaskViewModel(ICommentService commentService,
            IDialogService dialogService,
            INavigationService navigationService,
            IAttacmentService attacmentService,
            ILogger<TaskViewModel> logger,
            IDialogCoordinator dialogCoordinator)
        {
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
        }

        // Commands
        public ICommand SwitchToProjectCommand { get; }
        public ICommand EditTaskCommand { get; }
        public ICommand AddCommentCommand { get; }
        public ICommand AddAttachmentCommand { get; }
        public ICommand OpenAttachmentCommand { get; }

        // State
        public ProjectsInfo Project { get; set; } = null!;

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

        public ObservableCollection<Comments>? Comments { get; private set; } = new();
        public ObservableCollection<TaskAttachment>? Attachments { get; private set; } = new();

        // Inputs
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

        // Init
        public async Task InitializationAsync(params object[] parameters)
        {
            CurrentTask = parameters.OfType<TasksInfo>().FirstOrDefault()
                ?? throw new ArgumentNullException();

            Project = parameters.OfType<ProjectsInfo>().FirstOrDefault()
                ?? throw new ArgumentNullException();

            try
            {
                Comments = new ObservableCollection<Comments>(
                    await _commentService.GetComments(CurrentTask.Id));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не удалось загрузить комментарии (({mes}, {mes2})).", ex.Message, ex.InnerException);
                Comments = new ObservableCollection<Comments>();

                await _dialogCoordinator.ShowMessageAsync(this,
                    "Error",
                    "Comments are null",
                    MessageDialogStyle.Affirmative);
            }

            try
            {
                Attachments = new ObservableCollection<TaskAttachment>(
                    await _attacmentService.GetAttachmentsAsync(CurrentTask.Id));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не удалось загрузить вложения.");
                Attachments = new ObservableCollection<TaskAttachment>();

                await _dialogCoordinator.ShowMessageAsync(this,
                    "Error",
                    "Comments are null",
                    MessageDialogStyle.Affirmative);
            }
        }

        // Commands logic
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
                _logger.LogError(ex, "File not found {FilePath}", taskAttachment.FilePath);
                await _dialogCoordinator.ShowMessageAsync(this,
                    "Error",
                    "File not found",
                    MessageDialogStyle.Affirmative);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "File error {FilePath}", taskAttachment.FilePath);
                await _dialogCoordinator.ShowMessageAsync(this,
                    "Error",
                    "File error",
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
                _logger.LogError(ex, "Failed to add attachment for task {taskId}", CurrentTask.Id);
                await _dialogCoordinator.ShowMessageAsync(this,
                    "Error",
                    "Failed to add attachment. Try again",
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
                _logger.LogError(ex, "Failed to add comment for task {taskId}", CurrentTask.Id);
                await _dialogCoordinator.ShowMessageAsync(this, 
                    "Error",
                    "Failed to add comment. Try again",
                    MessageDialogStyle.Affirmative);
            }
        }

        private async Task EditTask()
        {
            await _dialogService.ShowTaskDialogAsync<
                TaskDialogView,
                EditTaskViewModel>(this, CurrentTask);
        }

        private void SwitchToProject() =>
            _navigationService.NavigateTo<ProjectViewModel>(Project);

        private bool CanSwitchToProject() => true;

        // Helpers
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