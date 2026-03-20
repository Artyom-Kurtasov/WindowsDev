using Microsoft.EntityFrameworkCore;
using WindowsDev.Businnes.DataBase;
using WindowsDev.Businnes.Services.ProjectService.Interfaces;
using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Businnes.Services.ProjectService
{
    public class ProjectReader : IProjectReader
    {
        private readonly AppDbContext _appDbContext;

        public ProjectReader(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<List<Project>> GetProjectsAsync()
        {
            return await _appDbContext.Project.ToListAsync();
        }
    }
}
