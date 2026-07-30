using Microsoft.EntityFrameworkCore;
using WindowsDev.Application.RepositoriesInterfaces;
using WindowsDev.Domain.Entities;
using WindowsDev.Infrastructure.Database.Interfaces;

namespace WindowsDev.Infrastructure.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly IDbCreator _dbManager;

        public ProjectRepository(IDbCreator dbManager)
        {
            _dbManager = dbManager;
        }

        public async Task<List<ProjectsInfo>> GetProjectsAsync(
            int page,
            int size,
            int userId,
            string searchFilter = ""
        )
        {
            using var dbContext = _dbManager.Create();

            return await dbContext
                .ProjectsInfo.Where(x =>
                    x.UserId == userId
                    && (
                        string.IsNullOrEmpty(searchFilter)
                        || x.Name.ToLower().Contains(searchFilter.ToLower())
                    )
                )
                .OrderBy(x => x.Id)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();
        }

        public async Task<int> GetProjectsCountAsync(int userId)
        {
            using var dbContext = _dbManager.Create();

            return await dbContext.ProjectsInfo.Where(x => x.UserId == userId).CountAsync();
        }

        public async Task AddAsync(ProjectsInfo project)
        {
            using var dbContext = _dbManager.Create();

            await dbContext.AddAsync(project);
            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            using var dbContext = _dbManager.Create();
            var project = await dbContext.ProjectsInfo.FindAsync(id);

            if (project is null)
                return;

            dbContext.ProjectsInfo.Remove(project);
            await dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(ProjectsInfo project)
        {
            using var dbContext = _dbManager.Create();
            dbContext.ProjectsInfo.Update(project);
            await dbContext.SaveChangesAsync();
        }
    }
}
