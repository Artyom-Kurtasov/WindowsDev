using WindowsDev.Domain.UsersAuthInfo;

namespace WindowsDev.Businnes.Services.ProjectService.Interfaces
{
    public interface IProjectWriter
    {
        Task AddAsync(ProjectsInfo project);
        Task UpdateAsync(ProjectsInfo project);
        Task DeleteAsync(int id);
    }
}
