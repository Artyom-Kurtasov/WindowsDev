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
using WindowsDev.View.Controls.WindowsContent;

namespace WindowsDev.ViewModels
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

        // Project associated with the current task.
        public ProjectsInfo Project { get; }

        // Current task being displayed or edited.
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
                OnPropertyChanged(nameof(NewComment));
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
                    OnPropertyChanged(nameof(Name));
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
                    OnPropertyChanged(nameof(Description));
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
                    OnPropertyChanged(nameof(Priority));
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
                    OnPropertyChanged(nameof(Progress));
                }
            }
        }

        /// <summary>
        /// Task status property.
        /// </summary>
        public string Status
        {
            get => CurrentTask.Status;
            set
            {
                if (CurrentTask != null && CurrentTask.Status != value)
                {
                    CurrentTask.Status = value;
                    OnPropertyChanged(nameof(Status));
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
                    OnPropertyChanged(nameof(DeadLine));
                }
            }
        }

        // Task creation date.
        public DateTime CreatedAt => CurrentTask.CreatedAt;

        // Collection of comments associated with the task.
        public ObservableCollection<Comments>? Comments => CurrentTask.Comments;

        // Collection of attachments associated with the task.
        public ObservableCollection<TaskAttachment>? Attachments => CurrentTask?.Attachments;

        // Command to switch back to the project view.
        public ICommand SwitchToProjectCommand { get; }

        // Command to open the task editing dialog.
        public ICommand EditTaskCommand { get; }

        // Command to add a new comment to the task.
        public ICommand AddCommentCommand { get; }

        // Command to add a file attachment to the task.
        public ICommand AddAttachmentCommand { get; }

        // Command to open an attachment file.
        public ICommand OpenAttachmentCommand { get; }

        // Constructor for TaskViewModel. Initializes services, commands, and subscribes to property changes.
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
                CurrentTask?.Comments?.Add(comment);
            }
        }

        /// <summary>
        /// Opens the dialog to edit the current task.
        /// </summary>
        private async Task EditTask()
        {
            await _dialogShowingService.ShowCreateDialogAsync<CreateTaskDialogView, TaskDialogViewModel>(this, CurrentTask);
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
