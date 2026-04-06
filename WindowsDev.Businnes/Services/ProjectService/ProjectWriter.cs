using WindowsDev.Businnes.DataBase;
using WindowsDev.Businnes.Services.ProjectService.Interfaces;
using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Businnes.Services.ProjectService
{
    /// <summary>
    /// Writes project data to the database and manages CRUD operations.
    /// </summary>
    public class ProjectWriter : IProjectWriter
    {
        private readonly AppDbContext _appDbContext;

        public ProjectWriter(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        /// <summary>
        /// Adds a new project to the database.
        /// </summary>
        public async Task AddAsync(ProjectsInfo project)
        {
            await _appDbContext.AddAsync(project);
            await _appDbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a project by its ID.
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            var project = await _appDbContext.ProjectsInfo.FindAsync(id);

            if (project != null)
            {
                _appDbContext.ProjectsInfo.Remove(project);
                await _appDbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Updates an existing project in the database.
        /// </summary>
        public async Task UpdateAsync(ProjectsInfo project)
        {
            _appDbContext.ProjectsInfo.Update(project);
            await _appDbContext.SaveChangesAsync();
        }
    }
}