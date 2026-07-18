using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WindowsDev.Domain.TasksModels.Enums;
using TaskStatus = WindowsDev.Domain.TasksModels.Enums.TaskStatus;

namespace WindowsDev.Domain.TasksModels
{
    public class TasksInfo
    {
        [Key]
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required TaskPriority Priority { get; set; }
        public required int Progress { get; set; }
        public required TaskStatus Status { get; set; }
        public required int ProjectId { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required DateTime DeadLine { get; set; }
        public ObservableCollection<Comments>? Comments { get; set; }
        public ObservableCollection<TaskAttachment>? Attachments { get; set; }

        [NotMapped]
        public bool IsSelected { get; set; }
        public DateTime DeadLineAtLocal => DeadLine.ToLocalTime();
    }
}


