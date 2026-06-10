using WindowsDev.Domain.TasksModels;

namespace WindowsDev.Business.Services.TaskService.Interfaces
{
    public interface ITaskService
    {
        Task<List<TasksInfo>> GetTasksAsync(TaskFilter filter);
        Task<int> GetTasksCountAsync(int projectId);
        Task<TasksInfo> GetTaskByIdAsync(int id);
        Task AddAsync(TasksInfo task);
        Task DeleteAsync(int id);
        Task UpdateAsync(TasksInfo task);
    }
}
