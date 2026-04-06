using Microsoft.EntityFrameworkCore;
using WindowsDev.Businnes.DataBase;
using WindowsDev.Businnes.Services.ProjectService.Interfaces;
using WindowsDev.Businnes.Services.UserManager;
using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Businnes.Services.ProjectService
{
    /// <summary>
    /// Reads projects from the database for the current user.
    /// </summary>
    public class ProjectReader : IProjectReader
    {
        private readonly AppDbContext _appDbContext;
        private readonly CurrentUserData _currentUserData;

        public ProjectReader(AppDbContext appDbContext, CurrentUserData currentUserData)
        {
            _appDbContext = appDbContext;
            _currentUserData = currentUserData;
        }

        /// <summary>
        /// Returns all projects belonging to the current user.
        /// </summary>
        public async Task<List<ProjectsInfo>> GetProjectsAsync()
        {
            return await _appDbContext.ProjectsInfo
                .Where(x => x.UserId == _currentUserData.UserId)
                .ToListAsync();
        }
    }
}