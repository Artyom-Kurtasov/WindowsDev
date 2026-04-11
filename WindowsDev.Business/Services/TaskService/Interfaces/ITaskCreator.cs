using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Business.Services.TaskService.Interfaces
{
    public interface ITaskCreator
    {
        Task CreateTask(TaskDTO taskDTO);
    }
}


