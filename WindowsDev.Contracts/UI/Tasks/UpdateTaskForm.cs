using WindowsDev.Domain.Enums;
using TaskStatus = WindowsDev.Domain.Enums.TaskStatus;

namespace WindowsDev.Contracts.UI.Tasks;

public record UpdateTaskForm(
    int Progress,
    string Name,
    string Description,
    TaskPriority Priority,
    TaskStatus Status,
    DateTime Deadline
);
