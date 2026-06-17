using WindowsDev.Business.Repositories.Interfaces;
using WindowsDev.Business.Services.TaskService.Interfaces;
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
            if (task is null)
                throw new Exception("TaskError_TaskIsNull");

            await _taskRepository.AddAsync(task);
        }

        public async Task DeleteAsync(int id)
        {
            var task = await _taskRepository.FindTaskById(id);

            if (task is null)
                throw new Exception("TaskError_TaskNotFound");

            await _taskRepository.DeleteAsync(task);
        }

        public async Task UpdateAsync(TasksInfo task)
        {
            if (task is null)
                throw new Exception("TaskError_TaskIsNull");

            await _taskRepository.UpdateAsync(task);
        }

        public async Task<List<TasksInfo>> GetTasksAsync(TaskFilter filter)
        {
            int page = filter.Page < 1 ? 1 : filter.Page;
            int pageSize = filter.PageSize < 1 ? 1 : filter.PageSize;
            int skip = (page - 1) * pageSize;

            return await _taskRepository.GetTasksAsync(filter, skip, pageSize);
        }

        public async Task<TasksInfo> GetTaskByIdAsync(int id)
        {
            if (id < 1)
                throw new Exception("TaskError_InvalidTaskId");

            var task = await _taskRepository.GetTaskByIdAsync(id);

            if (task is null)
                throw new Exception("TaskError_TaskNotFound");

            return task;
        }

        public async Task<int> GetTasksCountAsync(int projectId)
        {
            if (projectId < 1)
                return 0;

            return await _taskRepository.GetTasksCountAsync(projectId);
        }
    }
}
