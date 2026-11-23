using WindowsDev.Application.Primitives;
using WindowsDev.Contracts.UI.Projects;

namespace WindowsDev.ApiClient.ProjectsClient;

public interface IProjectsApiClient
{
    Task<Result<ProjectInfo>> AddAsync(
        ProjectForm request,
        CancellationToken cancellationToken = default
    );
    Task<Result<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<UpdateProjectInfo>> UpdateAsync(
        int id,
        UpdateProjectForm request,
        CancellationToken cancellationToken = default
    );
    Task<Result<List<ProjectInfo>>> GetProjectsAsync(
        int page,
        int pageSize,
        string? searchFilter = "",
        CancellationToken cancellationToken = default
    );
    Task<Result<int>> GetCountAsync(
        CancellationToken cancellationToken = default
    );
    Task<Result<ProjectInfo>> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default
    );
}
