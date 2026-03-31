using WindowsDev.Businnes.DataBase;
using WindowsDev.Businnes.Services.ProjectService.Interfaces;
using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Businnes.Services.ProjectService
{
    public class ProjectWriter : IProjectWriter
    {
        private readonly AppDbContext _appDbContext;

        public ProjectWriter(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task AddAsync(ProjectsInfo project)
        {
            await _appDbContext.AddAsync(project);
            await _appDbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var project = await _appDbContext.ProjectsInfo.FindAsync(id);

            if (project != null)
            {
                _appDbContext.ProjectsInfo.Remove(project);
                await _appDbContext.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(ProjectsInfo project)
        {
            _appDbContext.ProjectsInfo.Update(project);
            await _appDbContext.SaveChangesAsync();
        }
    }
}
