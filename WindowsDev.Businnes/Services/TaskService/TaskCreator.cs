using WindowsDev.Businnes.Services.TaskService.Interfaces;
using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Businnes.Services.TaskService
{
    public class TaskCreator : ITaskCreator
    {
        private readonly ITaskWriter _taskWriter;

        public TaskCreator(ITaskWriter taskWriter)
        {
            _taskWriter = taskWriter;
        }

        public async Task CreateTask(TaskDTO taskDTO)
        {
            TasksInfo task = new TasksInfo
            {
                Id = taskDTO.Id,
                Name = taskDTO.Name,
                Description = taskDTO.Description,
                Priority = taskDTO.Priority,
                Progress = taskDTO.Progress,
                Status = taskDTO.Status,
                CreatedAt = DateTime.Now.ToUniversalTime(),
                DeadLine = taskDTO.DeadLine,
                Comments = taskDTO.Comments,
                Attachments = taskDTO.Attachments
            };
            await _taskWriter.AddAsync(task);
        }
    }
}
