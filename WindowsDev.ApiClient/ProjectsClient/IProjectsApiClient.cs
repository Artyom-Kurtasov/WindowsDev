using WindowsDev.Application.Primitives;
using WindowsDev.Contracts.Api.Projects;

namespace WindowsDev.ApiClient.ProjectsClient;

public interface IProjectsApiClient
{
    Task<Result<AddProjectResponse>> AddAsync(
        AddProjectRequest request,
        CancellationToken cancellationToken = default
    );
    Task<Result<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<UpdateProjectResponse>> UpdateAsync(
        int id,
        UpdateProjectRequest request,
        CancellationToken cancellationToken = default
    );
    Task<Result<List<GetProjectsResponse>>> GetProjectsAsync(
        int page,
        int pageSize,
        string? searchFilter = "",
        CancellationToken cancellationToken = default
    );
    Task<Result<GetProjectsCountResponse>> GetCountAsync(
        CancellationToken cancellationToken = default
    );
    Task<Result<GetProjectsResponse>> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default
    );
}
