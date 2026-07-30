using WindowsDev.Domain.Entities;

namespace WindowsDev.Application.RepositoriesInterfaces
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
