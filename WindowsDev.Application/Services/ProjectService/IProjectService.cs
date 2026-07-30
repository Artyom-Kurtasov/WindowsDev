using WindowsDev.Domain.Entities;

namespace WindowsDev.Application.Services.ProjectService
{
    public interface IProjectService
    {
        Task AddAsync(ProjectsInfo project);
        Task UpdateAsync(ProjectsInfo project);
        Task DeleteAsync(int id);
        Task<List<ProjectsInfo>> GetProjectsAsync(int page, int size, string searchFilter = "");
        Task<int> GetProjectsCountAsync();
    }
}
