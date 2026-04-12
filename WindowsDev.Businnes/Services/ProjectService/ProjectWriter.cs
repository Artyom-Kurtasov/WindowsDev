using Microsoft.EntityFrameworkCore;
using WindowsDev.Business.DataBase;
using WindowsDev.Business.Services.ProjectService.Interfaces;
using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Business.Services.ProjectService
{
    /// <summary>
    /// Writes project data to the database and manages CRUD operations.
    /// </summary>
    public class ProjectWriter : IProjectWriter
    {
        private readonly DbManager _dbManager;

        public ProjectWriter(DbManager dbManager)
        {
            _dbManager = dbManager;
        }

        /// <summary>
        /// Adds a new project to the database.
        /// </summary>
        public async Task AddAsync(ProjectsInfo project)
        {
            using var dbContext = _dbManager.Create();
            await dbContext.AddAsync(project);
            await dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a project by its ID.
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            using var dbContext = _dbManager.Create();
            var project = await dbContext.ProjectsInfo.FindAsync(id);
            var relatedTasks = await dbContext.TasksInfo
                .Where(x => x.ProjectId == id)
                .ToListAsync();

            if (relatedTasks.Any())
            {
                dbContext.TasksInfo.RemoveRange(relatedTasks);
                await dbContext.SaveChangesAsync();
            }

            dbContext.ProjectsInfo.Remove(project);
            await dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Updates an existing project in the database.
        /// </summary>
        public async Task UpdateAsync(ProjectsInfo project)
        {
            using var dbContext = _dbManager.Create();
            dbContext.ProjectsInfo.Update(project);
            await dbContext.SaveChangesAsync();
        }
    }
}

