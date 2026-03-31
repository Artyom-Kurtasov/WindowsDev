using System.Collections.ObjectModel;
using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Businnes.Services.TaskService.Interfaces
{
    public interface ITaskLoader
    {
        Task<ObservableCollection<TasksInfo>> LoadTaskAsync();
    }
}
