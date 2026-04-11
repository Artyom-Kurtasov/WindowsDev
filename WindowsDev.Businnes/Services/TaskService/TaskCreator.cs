using WindowsDev.Business.Services.TaskService.Interfaces;
using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Business.Services.TaskService
{
    /// <summary>
    /// Handles task creation and storing it via TaskWriter.
    /// </summary>
    public class TaskCreator : ITaskCreator
    {
        private readonly ITaskWriter _taskWriter;

        public TaskCreator(ITaskWriter taskWriter)
        {
            _taskWriter = taskWriter;
        }

        /// <summary>
        /// Creates a new task from the provided TaskDTO.
        /// </summary>
        public async Task CreateTask(TaskDTO taskDto)
        {
            var task = new TasksInfo
            {
                Id = taskDto.Id,
                Name = taskDto.Name,
                Description = taskDto.Description,
                Priority = taskDto.Priority,
                Progress = taskDto.Progress,
                Status = taskDto.Status,
                CreatedAt = DateTime.UtcNow,
                DeadLine = taskDto.DeadLine,
                Comments = taskDto.Comments,
                Attachments = taskDto.Attachments,
                ProjectId = taskDto.ProjectId
            };

            await _taskWriter.AddAsync(task);
        }
    }
}
