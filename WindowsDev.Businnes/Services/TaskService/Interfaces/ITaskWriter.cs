using WindowsDev.Domain.TasksModels;

namespace WindowsDev.Business.Services.TaskService.Interfaces
{
    public interface ITaskWriter
    {
        Task AddAsync(TasksInfo task);
        Task DeleteAsync(int id);
        Task UpdateAsync(TasksInfo task);
    }
}


