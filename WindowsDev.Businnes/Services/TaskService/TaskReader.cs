using Microsoft.EntityFrameworkCore;
using WindowsDev.Business.DataBase;
using WindowsDev.Business.Services.TaskService.Interfaces;
using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Business.Services.TaskService
{
    /// <summary>
    /// Reads tasks from the database.
    /// </summary>
    public class TaskReader : ITaskReader
    {
        private readonly DbManager _dbManager;

        public TaskReader(DbManager dbManager)
        {
            _dbManager = dbManager;
        }

        /// <summary>
        /// Returns all tasks from the database asynchronously.
        /// </summary>
        public async Task<List<TasksInfo>> GetTasksAsync(int id)
        {
            using var dbContext = _dbManager.Create();
            return await dbContext.TasksInfo
                .Where(x => x.ProjectId == id)
                .ToListAsync();
        }
    }
}
