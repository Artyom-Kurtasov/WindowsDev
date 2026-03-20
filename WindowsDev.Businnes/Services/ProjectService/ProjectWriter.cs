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

        public async Task AddAsync(Project project)
        {
            await _appDbContext.AddAsync(project);
            await _appDbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var project = await _appDbContext.Project.FindAsync(id);

            if (project != null)
            {
                _appDbContext.Project.Remove(project);
                await _appDbContext.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(Project project)
        {
            _appDbContext.Project.Update(project);
            await _appDbContext.SaveChangesAsync();
        }
    }
}
