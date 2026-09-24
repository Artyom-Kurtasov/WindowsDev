using WindowsDev.Api.DTO.Request.ProjectsController;
using WindowsDev.Api.DTO.Request.ProjectService;
using WindowsDev.Api.DTO.Response.ProjectsController;
using WindowsDev.Application.Primitives;

namespace WindowsDev.ApiClients.ProjectsClient;

public interface IProjectsApiClient
{
    Task<Result<AddResponse>> AddAsync(
        AddProjectRequest request,
        CancellationToken cancellationToken = default
    );
    Task<Result<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<UpdateResponse>> UpdateAsync(
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
