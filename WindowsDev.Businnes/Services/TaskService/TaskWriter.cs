using WindowsDev.Businnes.DataBase;
using WindowsDev.Businnes.Services.TaskService.Interfaces;
using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Businnes.Services.TaskService
{
    /// <summary>
    /// Handles writing, updating, and deleting tasks in the database.
    /// </summary>
    public class TaskWriter : ITaskWriter
    {
        private readonly AppDbContext _appDbContext;

        public TaskWriter(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        /// <summary>
        /// Adds a new task to the database.
        /// </summary>
        public async Task AddAsync(TasksInfo task)
        {
            await _appDbContext.AddAsync(task);
            await _appDbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a task by its ID if it exists.
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            var task = await _appDbContext.TasksInfo.FindAsync(id);

            if (task != null)
            {
                _appDbContext.TasksInfo.Remove(task);
                await _appDbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Updates an existing task in the database.
        /// </summary>
        public async Task UpdateAsync(TasksInfo task)
        {
            var existingTask = await _appDbContext.TasksInfo.FindAsync(task.Id);

            if (existingTask != null)
            {
                existingTask.Name = task.Name;
                existingTask.Description = task.Description;
                existingTask.Progress = task.Progress;
                existingTask.Priority = task.Priority;
                existingTask.Status = task.Status;
                existingTask.DeadLine = task.DeadLine;

                await _appDbContext.SaveChangesAsync();
            }
        }
    }
}