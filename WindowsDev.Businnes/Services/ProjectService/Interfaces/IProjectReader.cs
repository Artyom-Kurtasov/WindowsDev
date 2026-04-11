using WindowsDev.Business.Services.UserManager;
using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Business.Services.ProjectService.Interfaces
{
    public interface IProjectReader
    {
        Task<List<ProjectsInfo>> GetProjectsAsync();
    }
}

