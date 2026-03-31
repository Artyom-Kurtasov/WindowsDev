using System.Collections.ObjectModel;

namespace WindowsDev.Domain.UsersAuthInfo
{
    public class TaskDTO
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required string Priority { get; set; }
        public required string Progress { get; set; }
        public required string Status { get; set; }
        public required DateTime DeadLine { get; set; }
        public ObservableCollection<Comments>? Comments { get; set; }
        public ObservableCollection<Attachment>? Attachments { get; set; }
    }
}
