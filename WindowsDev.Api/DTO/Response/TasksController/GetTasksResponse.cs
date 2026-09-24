using WindowsDev.Domain.Enums;
using TaskStatus = WindowsDev.Domain.Enums.TaskStatus;

namespace WindowsDev.Api.DTO.Response.TasksController
{
    public class GetTasksResponse
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required TaskPriority Priority { get; set; }
        public required int Progress { get; set; }
        public required TaskStatus Status { get; set; }
        public required int ProjectId { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required DateTime Deadline { get; set; }
    }
}
