using System.Collections.ObjectModel;
using WindowsDev.Domain.TasksModels;

namespace WindowsDev.Business.Services.TaskService.Interfaces
{
    public interface ITaskLoader
    {
        Task<ObservableCollection<TasksInfo>> LoadTaskAsync(int id);
    }
}


