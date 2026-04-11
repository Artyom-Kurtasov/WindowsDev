using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Business.Services.TaskService.Interfaces
{
    public interface ITaskReader
    {
        Task<List<TasksInfo>> GetTasksAsync(int id);
    }
}


