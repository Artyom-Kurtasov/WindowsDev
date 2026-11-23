using WindowsDev.Domain.Enums;
using TaskStatus = WindowsDev.Domain.Enums.TaskStatus;

namespace WindowsDev.Contracts.UI.Tasks;

public class TaskInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; }
    public int Progress { get; set; }
    public TaskStatus Status { get; set; }
    public int ProjectId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime Deadline { get; set; }
}
