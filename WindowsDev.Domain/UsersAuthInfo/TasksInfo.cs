using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace WindowsDev.Domain.UsersAuthInfo
{
    public class TasksInfo
    {
        [Key]
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required string Priority { get; set; }
        public required string Progress { get; set; }
        public required string Status { get; set; }
        public required int ProjectId { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required DateTime DeadLine { get; set; }
        public ObservableCollection<Comments>? Comments { get; set; }
        public ObservableCollection<TaskAttachment>? Attachments { get; set; }
    }
}


