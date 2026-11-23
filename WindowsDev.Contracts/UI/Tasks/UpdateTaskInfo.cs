using WindowsDev.Domain.Enums;
using TaskStatus = WindowsDev.Domain.Enums.TaskStatus;

namespace WindowsDev.Contracts.UI.Tasks;

public record UpdateTaskInfo(
    int Id,
    string Name,
    string? Description,
    TaskPriority Priority,
    TaskStatus Status,
    DateTime Deadline,
    DateTime CreatedAt,
    int Progress,
    int ProjectId
);

