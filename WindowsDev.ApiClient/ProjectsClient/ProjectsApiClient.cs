using AutoMapper;
using System.Net;
using System.Net.Http.Json;
using WindowsDev.Application.Identity.Cryptography;
using WindowsDev.Application.Primitives;
using WindowsDev.Contracts.Api.Projects;
using WindowsDev.Contracts.UI.Projects;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;

namespace WindowsDev.ApiClient.ProjectsClient;

public class ProjectsApiClient : ApiClientBase, IProjectsApiClient
{
    private readonly IMapper _mapper;

    public ProjectsApiClient(
        HttpClient httpClient,
        ISecureTokenStorage tokenStorage,
        IMapper mapper
    )
        : base(httpClient, tokenStorage)
    {
        _mapper = mapper;
    }

    public async Task<Result<ProjectInfo>> AddAsync(
        ProjectForm request,
        CancellationToken cancellationToken = default
    )
    {
        await AppendAuthorizationHeaderAsync(cancellationToken);
        var apiRequest = _mapper.Map<AddProjectRequest>(request);
        using var response = await _httpClient.PostAsJsonAsync(
            "api/projects",
            apiRequest,
            cancellationToken
        );

        var result = await response.Content.ReadFromJsonAsync<AddProjectResponse>(
            cancellationToken: cancellationToken
        );

        var projectInfo = _mapper.Map<ProjectInfo>(result);
        return Result<ProjectInfo>.Success(projectInfo);
    }

    public async Task<Result<bool>> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        await AppendAuthorizationHeaderAsync(cancellationToken);
        using var response = await _httpClient.DeleteAsync($"api/projects/{id}", cancellationToken);

        return Result<bool>.Success(true);
    }

    public async Task<Result<UpdateProjectInfo>> UpdateAsync(
        int id,
        UpdateProjectForm request,
        CancellationToken cancellationToken = default
    )
    {
        await AppendAuthorizationHeaderAsync(cancellationToken);
        using var response = await _httpClient.PatchAsJsonAsync(
            $"api/projects/{id}",
            request,
            cancellationToken
        );

        if (response.StatusCode == HttpStatusCode.NotFound)
            return Result<UpdateProjectInfo>.Failure(ProjectsErrors.ProjectNotFound);

        var result = await response.Content.ReadFromJsonAsync<UpdateProjectResponse>(
            cancellationToken: cancellationToken
        );

        var info = _mapper.Map<UpdateProjectInfo>(result);
        return Result<UpdateProjectInfo>.Success(info);
    }

    public async Task<Result<List<ProjectInfo>>> GetProjectsAsync(
        int page,
        int pageSize,
        string? searchFilter = "",
        CancellationToken cancellationToken = default
    )
    {
        await AppendAuthorizationHeaderAsync(cancellationToken);
        var search = Uri.EscapeDataString(searchFilter ?? string.Empty);
        var url = $"api/projects?page={page}&pageSize={pageSize}&searchFilter={search}";

        using var response = await _httpClient.GetAsync(url, cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<List<GetProjectsResponse>>(
            cancellationToken: cancellationToken
        );

        var info = _mapper.Map<List<ProjectInfo>>(result);
        return Result<List<ProjectInfo>>.Success(info);
    }

    public async Task<Result<int>> GetCountAsync(CancellationToken cancellationToken = default)
    {
        await AppendAuthorizationHeaderAsync(cancellationToken);
        using var response = await _httpClient.GetAsync("api/projects/count", cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<GetProjectsCountResponse>(
            cancellationToken: cancellationToken
        );
        return Result<int>.Success(result.TotalCount);
    }

    public async Task<Result<ProjectInfo>> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        await AppendAuthorizationHeaderAsync(cancellationToken);
        using var response = await _httpClient.GetAsync($"api/projects/{id}", cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<GetProjectsResponse>(
            cancellationToken: cancellationToken
        );

        var info = _mapper.Map<ProjectInfo>(result);
        return Result<ProjectInfo>.Success(info);
    }
}
