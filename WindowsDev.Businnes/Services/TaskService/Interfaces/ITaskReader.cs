using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Businnes.Services.TaskService.Interfaces
{
    public interface ITaskReader
    {
        Task<List<TasksInfo>> GetTasksAsync();
    }
}
