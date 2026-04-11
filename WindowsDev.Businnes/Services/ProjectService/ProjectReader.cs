using Microsoft.EntityFrameworkCore;
using WindowsDev.Business.DataBase;
using WindowsDev.Business.Services.ProjectService.Interfaces;
using WindowsDev.Business.Services.UserManager;
using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Business.Services.ProjectService
{
    /// <summary>
    /// Reads projects from the database for the current user.
    /// </summary>
    public class ProjectReader : IProjectReader
    {
        private readonly DbManager _dbManager;
        private readonly CurrentUserData _currentUserData;

        public ProjectReader(DbManager dbManager, CurrentUserData currentUserData)
        {
            _dbManager = dbManager;
            _currentUserData = currentUserData;
        }

        /// <summary>
        /// Returns all projects belonging to the current user.
        /// </summary>
        public async Task<List<ProjectsInfo>> GetProjectsAsync()
        {
            using var dbContext = _dbManager.Create();

            return await dbContext.ProjectsInfo
                .Where(x => x.UserId == _currentUserData.UserId)
                .ToListAsync();
        }
    }
}

