using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Businnes.Services.TaskService.Interfaces
{
    public interface ITaskWriter
    {
        Task AddAsync(TasksInfo task);
        Task DeleteAsync(int id);
        Task UpdateAsync(TasksInfo task);
    }
}
