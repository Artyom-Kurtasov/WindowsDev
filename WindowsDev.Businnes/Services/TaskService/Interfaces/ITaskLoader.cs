using System.Collections.ObjectModel;
using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Business.Services.TaskService.Interfaces
{
    public interface ITaskLoader
    {
        Task<ObservableCollection<TasksInfo>> LoadTaskAsync(int id);
    }
}


