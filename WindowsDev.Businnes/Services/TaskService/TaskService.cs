using WindowsDev.Business.Primitives;
using WindowsDev.Business.Repositories.Interfaces;
using WindowsDev.Business.Services.TaskService.Interfaces;
using WindowsDev.Domain.DialogsMessages.Errors;
using WindowsDev.Domain.TasksModels;

namespace WindowsDev.Business.Services.TaskService
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;

        public TaskService(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public async Task AddAsync(TasksInfo task)
        {
            ArgumentNullException.ThrowIfNull(task);

            await _taskRepository.AddAsync(task);
        }

        public async Task DeleteAsync(int id)
        {
            var task = await _taskRepository.FindTaskById(id);

            ArgumentNullException.ThrowIfNull(task);

            await _taskRepository.DeleteAsync(task);
        }

        public async Task UpdateAsync(TasksInfo task)
        {
            ArgumentNullException.ThrowIfNull(task);

            await _taskRepository.UpdateAsync(task);
        }

        public async Task<Result<List<TasksInfo>>> GetTasksAsync(TaskFilter filter)
        {
            int page = filter.Page < 1 ? 1 : filter.Page;
            int pageSize = filter.PageSize < 1 ? 1 : filter.PageSize;
            int skip = (page - 1) * pageSize;

            var tasks = await _taskRepository.GetTasksAsync(filter, skip, pageSize);
            return Result<List<TasksInfo>>.Success(tasks);
        }

        public async Task<int> GetTasksCountAsync(int projectId)
        {
            if (projectId < 1)
                return 0;

            return await _taskRepository.GetTasksCountAsync(projectId);
        }
    }
}
