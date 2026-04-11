using WindowsDev.Business.DataBase;
using WindowsDev.Business.Services.TaskService.Interfaces;
using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Business.Services.TaskService
{
    /// <summary>
    /// Handles writing, updating, and deleting tasks in the database.
    /// </summary>
    public class TaskWriter : ITaskWriter
    {
        private readonly DbManager _dbManager;

        public TaskWriter(DbManager dbManager)
        {
            _dbManager = dbManager;
        }

        /// <summary>
        /// Adds a new task to the database.
        /// </summary>
        public async Task AddAsync(TasksInfo task)
        {
            using var dbContext = _dbManager.Create();

            await dbContext.AddAsync(task);
            await dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a task by its ID if it exists.
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            using var dbContext = _dbManager.Create();
            var task = await dbContext.TasksInfo.FindAsync(id);

            if (task != null)
            {
                dbContext.TasksInfo.Remove(task);
                await dbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Updates an existing task in the database.
        /// </summary>
        public async Task UpdateAsync(TasksInfo task)
        {
            using var dbContext = _dbManager.Create();
            var existingTask = await dbContext.TasksInfo.FindAsync(task.Id);

            if (existingTask != null)
            {
                existingTask.Name = task.Name;
                existingTask.Description = task.Description;
                existingTask.Progress = task.Progress;
                existingTask.Priority = task.Priority;
                existingTask.Status = task.Status;
                existingTask.DeadLine = task.DeadLine;

                await dbContext.SaveChangesAsync();
            }
        }
    }
}
