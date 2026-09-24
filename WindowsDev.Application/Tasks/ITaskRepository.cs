using WindowsDev.Domain.Entities;

namespace WindowsDev.Application.Tasks;

public interface ITaskRepository
{
    Task<List<TasksInfo>> GetTasksAsync(TaskFilter filter, int size, int pageSize);
    Task<TasksInfo?> GetAsync(int id);
    Task<int> GetCountAsync(int projectId);
    Task AddAsync(TasksInfo task);
    Task DeleteAsync(TasksInfo task);
    Task UpdateAsync(TasksInfo task);
    Task<TasksInfo?> FindTaskById(int id);
}