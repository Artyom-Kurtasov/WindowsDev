using WindowsDev.Domain.Entities;

namespace WindowsDev.Application.Projects;

public interface IProjectRepository
{
    Task AddAsync(ProjectsInfo project);
    Task UpdateAsync(ProjectsInfo project);
    Task DeleteAsync(int id);
    Task<List<ProjectsInfo>> GetProjectsAsync(int page, int size, int userId, string searchFilter = "");
    Task<int> GetProjectsCountAsync(int userId);
    Task<ProjectsInfo> GetProjectById(int projectId);
}