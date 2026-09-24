using WindowsDev.Application.Primitives;
using WindowsDev.Domain.Entities;

namespace WindowsDev.Application.Tasks;

public interface ITaskService
{
    Task<Result<List<TasksInfo>>> GetTasksAsync(TaskFilter filter);
    Task<int> GetTasksCountAsync(int projectId);
    Task AddAsync(TasksInfo task);
    Task DeleteAsync(int id);
    Task UpdateAsync(TasksInfo task);
    Task<TasksInfo?> GetAsync(int id);
}