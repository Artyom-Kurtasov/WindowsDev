using WindowsDev.Domain.TasksModels;

namespace WindowsDev.Business.Services.TaskService.Interfaces
{
    public interface ITaskReader
    {
        Task<List<TasksInfo>> GetTasksAsync(int id);
    }
}


