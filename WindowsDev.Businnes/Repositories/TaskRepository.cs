using Microsoft.EntityFrameworkCore;
using WindowsDev.Business.DataBase.Interfaces;
using WindowsDev.Business.Repositories.Interfaces;
using WindowsDev.Business.Services.TaskService;
using WindowsDev.Domain.TasksModels;


namespace WindowsDev.Business.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly IDbManager _dbManager;

        public TaskRepository(IDbManager dbManager)
        {
            _dbManager = dbManager;
        }

        public async Task AddAsync(TasksInfo task)
        {
            using var dbContext = _dbManager.Create();

            await dbContext.AddAsync(task);
            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(TasksInfo task)
        {
            using var dbContext = _dbManager.Create();

            dbContext.TasksInfo.Remove(task);
            await dbContext.SaveChangesAsync();
        }

        public async Task<TasksInfo> FindTaskById(int id)
        {
            using var dbContext = _dbManager.Create();
            var task = await dbContext.TasksInfo.FindAsync(id);

            return task;
        }

        public async Task<TasksInfo> GetTaskByIdAsync(int id)
        {
            using var dbContext = _dbManager.Create();

            var task = await dbContext.TasksInfo
                .FirstOrDefaultAsync(x => x.Id == id);

            return task;
        }

        public async Task<List<TasksInfo>> GetTasksAsync(TaskFilter filter, int skip, int pageSize)
        {
            using var dbContext = _dbManager.Create();

            var query = dbContext.TasksInfo.AsQueryable();

            query = query.Where(x => x.ProjectId == filter.ProjectId);

            if (!string.IsNullOrWhiteSpace(filter.Seacrh))
            {
                query = query.Where(x => x.Name.Contains(filter.Seacrh));
            }

            if (filter.Statuses.Any() == true)
            {
                query = query.Where(x => filter.Statuses.Contains(x.Status));
            }

            return await
                query
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTasksCountAsync(int projectId)
        {
            using var dbContext = _dbManager.Create();
            return await dbContext.TasksInfo
                .CountAsync(x => x.ProjectId == projectId);
        }

        public async Task UpdateAsync(TasksInfo task)
        {
            using var dbContext = _dbManager.Create();
            var existingTask = await FindTaskById(task.Id);

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
