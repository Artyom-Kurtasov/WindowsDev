using Microsoft.EntityFrameworkCore;
using WindowsDev.Businnes.DataBase;
using WindowsDev.Businnes.Services.TaskService.Interfaces;
using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Businnes.Services.TaskService
{
    /// <summary>
    /// Reads tasks from the database.
    /// </summary>
    public class TaskReader : ITaskReader
    {
        private readonly AppDbContext _appDbContext;

        public TaskReader(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        /// <summary>
        /// Returns all tasks from the database asynchronously.
        /// </summary>
        public async Task<List<TasksInfo>> GetTasksAsync()
        {
            return await _appDbContext.TasksInfo.ToListAsync();
        }
    }
}