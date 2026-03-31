using WindowsDev.Businnes.Services.UserManager;
using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Businnes.Services.ProjectService.Interfaces
{
    public interface IProjectReader
    {
        Task<List<ProjectsInfo>> GetProjectsAsync();
    }
}
