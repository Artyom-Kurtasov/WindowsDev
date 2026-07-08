using WindowsDev.Business.Primitives;
using WindowsDev.Domain.ProjectsModels;

namespace WindowsDev.Business.Services.ProjectService.Interfaces
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
