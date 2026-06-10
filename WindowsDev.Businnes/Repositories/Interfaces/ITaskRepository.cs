using WindowsDev.Business.Services.TaskService;
using WindowsDev.Domain.TasksModels;

namespace WindowsDev.Business.Repositories.Interfaces
{
    public interface ITaskRepository
    {
        Task<List<TasksInfo>> GetTasksAsync(TaskFilter filter, int size, int pageSize);
        Task<TasksInfo> GetTaskByIdAsync(int id);
        Task<int> GetTasksCountAsync(int projectId);
        Task AddAsync(TasksInfo task);
        Task DeleteAsync(TasksInfo task);
        Task UpdateAsync(TasksInfo task);
        Task<TasksInfo> FindTaskById(int id);
    }
}
