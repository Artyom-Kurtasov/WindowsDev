using WindowsDev.Domain;
using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.ViewModels
{
    public class TaskViewModel : ViewModelBase
    {
        public TaskItem CurrentTask {  get; }

        public string? Name => CurrentTask?.Name;
        public string? Description => CurrentTask?.Description;
        public string? Created => CurrentTask?.Created.ToString();
        public string? DeadLine => CurrentTask?.DeadLine.ToString();
        public ICollection<Comment>? Comments => CurrentTask?.Comments;
        public ICollection<Attachment>? Attachments => CurrentTask?.Attachments;

        public TaskViewModel(TaskItem taskItem)
        {
            CurrentTask = taskItem;
        }
    }
}
