using WindowsDev.Domain.Entities;

namespace WindowsDev.Application.RepositoriesInterfaces
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
