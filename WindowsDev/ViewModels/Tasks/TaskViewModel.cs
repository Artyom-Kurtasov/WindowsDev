using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using WindowsDev.Business.Services;
using WindowsDev.Business.Services.TaskService;
using WindowsDev.Business.Services.TaskService.Attachment;
using WindowsDev.Commands.NavigationManager.Interfaces;
using WindowsDev.Domain;
using WindowsDev.Domain.UsersAuthInfo;
using WindowsDev.Infrastructure;
using WindowsDev.ViewModels.Projects;
using WindowsDev.Views.Tasks;

namespace WindowsDev.ViewModels.Tasks
{
    /// <summary>
    /// ViewModel for displaying and managing a single task, including comments, attachments, and editing.
    /// Implements IDisposable to unsubscribe from events.
    /// </summary>
    public class TaskViewModel : ViewModelBase, IDisposable
    {
        private readonly INavigationService _navigationService;
        private readonly DialogShowingService _dialogShowingService;
        private readonly SharedDataService _sharedDataService;
        private readonly FileWriter _fileWriter;
        private readonly AddComment _addComment;

        /// <summary>
        /// Project associated with the current task.
        /// </summary>
        public ProjectsInfo Project { get; }

        /// <summary>
        /// Current task being displayed or edited.
        /// </summary>
        public TasksInfo CurrentTask
        {
            get => _sharedDataService.CurrentTask;
            set
            {
                _sharedDataService.CurrentTask = value;
                OnPropertyChanged(nameof(CurrentTask));
            }
        }

        private string? _newComment;

        /// <summary>
        /// New comment text input by the user.
        /// </summary>
        public string? NewComment
        {
            get => _newComment;
            set
            {
                _newComment = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Task name property.
        /// </summary>
        public string Name
        {
            get => CurrentTask.Name;
            set
            {
                if (CurrentTask != null && CurrentTask.Name != value)
                {
                    CurrentTask.Name = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Task description property.
        /// </summary>
        public string? Description
        {
            get => CurrentTask?.Description;
            set
            {
                if (CurrentTask != null && CurrentTask.Description != value)
                {
                    CurrentTask.Description = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Task priority property.
        /// </summary>
        public string Priority
        {
            get => CurrentTask.Priority;
            set
            {
                if (CurrentTask != null && CurrentTask.Priority != value)
                {
                    CurrentTask.Priority = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Task progress property.
        /// </summary>
        public string Progress
        {
            get => CurrentTask.Progress;
            set
            {
                if (CurrentTask != null && CurrentTask.Progress != value)
                {
                    CurrentTask.Progress = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Task status property.
        /// </summary>
        public string? Status
        {
            get => CurrentTask?.Status;
            set
            {
                if (CurrentTask != null && CurrentTask.Status != value)
                {
                    CurrentTask.Status = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Task deadline property.
        /// </summary>
        public DateTime DeadLine
        {
            get => CurrentTask.DeadLine;
            set
            {
                if (CurrentTask != null && CurrentTask.DeadLine != value)
                {
                    CurrentTask.DeadLine = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Task creation date.
        /// </summary>
        public DateTime CreatedAt => CurrentTask.CreatedAt;

        /// <summary>
        /// Collection of comments associated with the task.
        /// </summary>
        public ObservableCollection<Comments>? Comments => CurrentTask?.Comments;

        /// <summary>
        /// Collection of attachments associated with the task.
        /// </summary>
        public ObservableCollection<TaskAttachment>? Attachments => CurrentTask?.Attachments;

        /// <summary>
        /// Command to switch back to the project view.
        /// </summary>
        public ICommand SwitchToProjectCommand { get; }

        /// <summary>
        /// Command to open the task editing dialog.
        /// </summary>
        public ICommand EditTaskCommand { get; }

        /// <summary>
        /// Command to add a new comment to the task.
        /// </summary>
        public ICommand AddCommentCommand { get; }

        /// <summary>
        /// Command to add a file attachment to the task.
        /// </summary>
        public ICommand AddAttachmentCommand { get; }

        /// <summary>
        /// Command to open an attachment file.
        /// </summary>
        public ICommand OpenAttachmentCommand { get; }

        /// <summary>
        /// Constructor for TaskViewModel. Initializes services, commands, and subscribes to property changes.
        /// </summary>
        public TaskViewModel(
            AddComment addComment,
            DialogShowingService dialogShowingService,
            INavigationService navigationService,
            ProjectsInfo project,
            SharedDataService sharedDataService,
            FileWriter fileWriter)
        {
            _addComment = addComment;
            _dialogShowingService = dialogShowingService;
            _navigationService = navigationService;
            _sharedDataService = sharedDataService;
            _fileWriter = fileWriter;
            Project = project;

            _sharedDataService.PropertyChanged += OnSharedDataChanged;

            AddCommentCommand = new AsyncRelayCommand(AddComment);
            EditTaskCommand = new AsyncRelayCommand(EditTask);
            SwitchToProjectCommand = new RelayCommand(SwitchToProject, CanSwitchToProject);
            AddAttachmentCommand = new AsyncRelayCommand(AddAttachment);
            OpenAttachmentCommand = new AsyncRelayCommand<TaskAttachment>(OpenAttachment);
        }

        /// <summary>
        /// Handles property changes in SharedDataService to update bindings.
        /// </summary>
        private void OnSharedDataChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SharedDataService.CurrentTask))
            {
                OnPropertyChanged(nameof(CurrentTask));
                OnPropertyChanged(nameof(Name));
                OnPropertyChanged(nameof(Description));
                OnPropertyChanged(nameof(Priority));
                OnPropertyChanged(nameof(Progress));
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(CreatedAt));
                OnPropertyChanged(nameof(DeadLine));
                OnPropertyChanged(nameof(Comments));
                OnPropertyChanged(nameof(Attachments));
            }
        }

        /// <summary>
        /// Opens the selected attachment file using the default application.
        /// </summary>
        private async Task OpenAttachment(TaskAttachment taskAttachment)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = taskAttachment.FilePath,
                UseShellExecute = true
            });
        }

        /// <summary>
        /// Opens a file dialog to select a file and adds it as an attachment.
        /// </summary>
        private async Task AddAttachment()
        {
            OpenFileDialog dialog = new OpenFileDialog();

            if (dialog.ShowDialog() == true)
            {
                await _fileWriter.AddFileInfoToDatavase(dialog.FileName);
            }
        }

        /// <summary>
        /// Adds a new comment to the current task if text is provided.
        /// </summary>
        private async Task AddComment()
        {
            if (!string.IsNullOrWhiteSpace(_newComment))
            {
                var comment = await _addComment.AddComments(CurrentTask, _newComment);
                CurrentTask.Comments.Add(comment);
            }
        }

        /// <summary>
        /// Opens the dialog to edit the current task.
        /// </summary>
        private async Task EditTask()
        {
            await _dialogShowingService.ShowCreateDialogAsync<TaskDialogView, TaskDialogViewModel>(this, CurrentTask);
        }

        /// <summary>
        /// Navigates back to the project view.
        /// </summary>
        private void SwitchToProject()
        {
            _navigationService.NavigateTo<ProjectViewModel>(Project);
        }

        /// <summary>
        /// Determines if switching to the project view is allowed.
        /// </summary>
        private bool CanSwitchToProject() => true;

        /// <summary>
        /// Unsubscribes from events to prevent memory leaks.
        /// </summary>
        public void Dispose()
        {
            _sharedDataService.PropertyChanged -= OnSharedDataChanged;
        }
    }
}