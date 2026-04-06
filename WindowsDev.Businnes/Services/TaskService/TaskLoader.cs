using System.Collections.ObjectModel;
using WindowsDev.Businnes.Services.TaskService.Interfaces;
using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Businnes.Services.TaskService
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
        public async Task<ObservableCollection<TasksInfo>> LoadTaskAsync()
        {
            var taskList = await _taskReader.GetTasksAsync();
            return new ObservableCollection<TasksInfo>(taskList);
        }
    }
}