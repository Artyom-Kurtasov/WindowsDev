using WindowsDev.Domain.Entities;

namespace WindowsDev.Application.Projects;

internal class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;

    public ProjectService(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task AddAsync(ProjectsInfo project)
    {
        ArgumentNullException.ThrowIfNull(project);

        await _projectRepository.AddAsync(project);
    }

    public async Task DeleteAsync(int id)
    {
        await _projectRepository.DeleteAsync(id);
    }

    public async Task UpdateAsync(ProjectsInfo project)
    {
        ArgumentNullException.ThrowIfNull(project);

        await _projectRepository.UpdateAsync(project);
    }

    public async Task<List<ProjectsInfo>> GetProjectsAsync(
        int userId,
        int page,
        int size,
        string searchFilter = ""
    )
    {
        page = page < 1 ? 1 : page;
        size = size < 1 ? 1 : size;

        return await _projectRepository.GetProjectsAsync(
            page,
            size,
            userId,
            searchFilter
        );
    }

    public async Task<int> GetProjectsCountAsync(int userId) =>
        await _projectRepository.GetProjectsCountAsync(userId);

    public async Task<ProjectsInfo> GetProjectById(int projectId)
    {
        return await _projectRepository.GetProjectById(projectId);
    }
}