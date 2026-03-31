using System.Collections.ObjectModel;
using WindowsDev.Businnes.Services.TaskService.Interfaces;
using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Businnes.Services.TaskService
{
    public class TaskLoader : ITaskLoader
    {
        private readonly ITaskReader _taskReader;

        public TaskLoader(ITaskReader taskReader)
        {
            _taskReader = taskReader;
        }

        public async Task<ObservableCollection<TasksInfo>> LoadTaskAsync()
        {
            var TaskList = await _taskReader.GetTasksAsync();
            return new ObservableCollection<TasksInfo>(TaskList);
        }
    }
}
