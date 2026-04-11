using System.Collections.ObjectModel;
using WindowsDev.Business.Services.TaskService.Interfaces;
using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Business.Services.TaskService
{
    /// <summary>
    /// Loads tasks via TaskReader and returns as ObservableCollection.
    /// </summary>
    public class TaskLoader : ITaskLoader
    {
        private readonly ITaskReader _taskReader;

        public TaskLoader(ITaskReader taskReader)
        {
            _taskReader = taskReader;
        }

        /// <summary>
        /// Loads all tasks asynchronously.
        /// </summary>
        public async Task<ObservableCollection<TasksInfo>> LoadTaskAsync(int id)
        {
            var taskList = await _taskReader.GetTasksAsync(id);
            return new ObservableCollection<TasksInfo>(taskList);
        }
    }
}
