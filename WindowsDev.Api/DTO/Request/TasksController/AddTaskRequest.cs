using System.ComponentModel.DataAnnotations;
using WindowsDev.Domain.Enums;
using TaskStatus = WindowsDev.Domain.Enums.TaskStatus;

namespace WindowsDev.Api.DTO.Request.TasksController;

public class AddTaskRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    public string Description {  get; set; } = string.Empty;
    [Required]
    public TaskPriority Priority { get; set; }
    [Required]
    public TaskStatus Status { get; set; }
    [Required] 
    
    public DateTime Deadline { get; set; }
    public int Progress { get; set; }
    public int ProjectId { get; set; }

}