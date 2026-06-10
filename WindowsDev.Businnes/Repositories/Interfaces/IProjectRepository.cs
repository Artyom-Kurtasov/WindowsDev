using WindowsDev.Domain.ProjectsModels;

namespace WindowsDev.Business.Repositories.Interfaces
{
    public interface IProjectRepository
    {
        Task AddAsync(ProjectsInfo project);
        Task UpdateAsync(ProjectsInfo project);
        Task DeleteAsync(int id);
        Task<List<ProjectsInfo>> GetProjectsAsync(int page, int size, int userId, string searchFilter = "");
        Task<int> GetProjectsCountAsync(int userId);
    }
}
