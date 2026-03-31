using System.Collections.ObjectModel;
using System.Windows.Input;
using WindowsDev.Businnes.Services;
using WindowsDev.Businnes.Services.TaskService;
using WindowsDev.Domain;
using WindowsDev.Domain.UsersAuthInfo;
using WindowsDev.Infrastructure;
using WindowsDev.View.Controls.WindowsContent;

namespace WindowsDev.ViewModels
{
    public class TaskViewModel : ViewModelBase
    {
        private readonly DialogShowingService _dialogShowingService;

        private TasksInfo _currentTask;
        public TasksInfo CurrentTask
        {
            get => _currentTask;
            private set
            {
                _currentTask = value;
                OnPropertyChanged(null);
            }
        }
        private readonly AddComment _addComment;

        public string? Name => CurrentTask?.Name;
        public string? Description => CurrentTask?.Description;
        public string? Priority => CurrentTask?.Priority;
        public string? Progress => CurrentTask?.Progress;
        public string? Status => CurrentTask?.Status;
        public string? Created => CurrentTask?.CreatedAt.ToString();
        public string? DeadLine => CurrentTask?.DeadLine.ToString();
        public ObservableCollection<Comments>? Comments => CurrentTask?.Comments;
        public ObservableCollection<Attachment>? Attachments => CurrentTask?.Attachments;

        private string? _newComment;
        public string? NewComment
        {
            get => _newComment;
            set
            {
                _newComment = value;
                OnPropertyChanged();
            }
        }

        public ICommand EditTaskCommand { get; }
        public ICommand AddCommentCommand { get; }
        public TaskViewModel(TasksInfo taskItem, AddComment addComment, DialogShowingService dialogShowingService)
        {
            CurrentTask = taskItem;
            _addComment = addComment;
            _dialogShowingService = dialogShowingService;

            AddCommentCommand = new AsyncRelayCommand(AddComment);
        }

        private async Task AddComment()
        {
            if (!string.IsNullOrEmpty(_newComment))
            {
                await _addComment.AddComments(CurrentTask, _newComment);
            }
        }
    }
}
