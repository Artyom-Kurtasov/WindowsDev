using WindowsDev.Domain.Entities;

namespace WindowsDev.Application.Projects;

public interface IProjectService
{
    Task AddAsync(ProjectsInfo project);
    Task UpdateAsync(ProjectsInfo project);
    Task DeleteAsync(int id);
    Task<List<ProjectsInfo>> GetProjectsAsync(int userId, int page, int size, string searchFilter = "");
    Task<int> GetProjectsCountAsync(int userId);
    Task<ProjectsInfo> GetProjectById(int projectId);
}