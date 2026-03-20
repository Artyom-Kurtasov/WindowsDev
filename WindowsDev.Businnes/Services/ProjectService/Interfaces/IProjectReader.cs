using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Businnes.Services.ProjectService.Interfaces
{
    public interface IProjectReader
    {
        Task<List<Project>> GetProjectsAsync();
    }
}
