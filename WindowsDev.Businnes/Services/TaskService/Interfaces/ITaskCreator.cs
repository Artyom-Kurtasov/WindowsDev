using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Businnes.Services.TaskService.Interfaces
{
    public interface ITaskCreator
    {
        Task CreateTask(TaskDTO taskDTO);
    }
}
