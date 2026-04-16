using System.Windows.Input;
using WindowsDev.Business.Services.ProjectService.Interfaces;
using WindowsDev.Business.Services.TaskService.Interfaces;
using WindowsDev.Domain.TasksModels;
using WindowsDev.Infrastructure;
using WindowsDev.ViewModels.Interfaces;

namespace WindowsDev.ViewModels.Tasks.Dialog
{
    public class EditTaskViewModel : TaskDialogViewModelBase, IInitializableAsync, IProjectDialogCreator
    {
        private readonly ITaskWriter _taskWriter;

        public event Func<Task>? CloseRequested;
        public TasksInfo? CurrentTask { get; private set; }

        public ICommand EditTaskCommand { get; }

        public EditTaskViewModel (ITaskWriter taskWriter)
        {
            _taskWriter = taskWriter;

            EditTaskCommand = new AsyncRelayCommand(EditTask);
        }

        public async Task InitializationAsync(params object[] parameter)
        {
            var task = parameter.OfType<TasksInfo>().FirstOrDefault();

            if (task != null)
            {
                CurrentTask = task;
                SetEditDialog(task);
            }
        }

        /// <summary>
        /// Edit existing task and refresh shared data.
        /// </summary>
        private async Task<TasksInfo> EditTask()
        {
            var existedTask = CurrentTask;

            if (existedTask != null)
            {
                existedTask.Name = Name;
                existedTask.Description = Description;
                existedTask.Priority = Priority;
                existedTask.Progress = Progress;
                existedTask.Status = Status;
                existedTask.DeadLine = DeadLine.ToUniversalTime();

                await _taskWriter.UpdateAsync(existedTask);

                if (CloseRequested != null)
                    await CloseRequested.Invoke();

                IsEditMode = false;

                return existedTask;
            }

            return CurrentTask;
        }

        private void SetEditDialog(TasksInfo task)
        {
            CurrentTask = task;
            Name = task.Name;
            Description = task.Description;
            Priority = task.Priority;
            Progress = task.Progress;
            Status = task.Status;
            DeadLine = task.DeadLine;

            IsEditMode = true;
        }
    }
}
