using WindowsDev.Business.Primitives;
using WindowsDev.Domain.TasksModels;

namespace WindowsDev.Business.Services.TaskService.Interfaces
{
    public interface ITaskService
    {
        Task<Result<List<TasksInfo>>> GetTasksAsync(TaskFilter filter);
        Task<int> GetTasksCountAsync(int projectId);
        Task AddAsync(TasksInfo task);
        Task DeleteAsync(int id);
        Task UpdateAsync(TasksInfo task);
    }
}
