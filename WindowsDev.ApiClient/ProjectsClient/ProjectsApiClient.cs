using System.Net;
using System.Net.Http.Json;
using WindowsDev.Application.Identity.Cryptography;
using WindowsDev.Application.Primitives;
using WindowsDev.Contracts.Api.Projects;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;

namespace WindowsDev.ApiClient.ProjectsClient;

public class ProjectsApiClient : IProjectsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ISecureTokenStorage _tokenStorage;

    public ProjectsApiClient(HttpClient httpClient, ISecureTokenStorage tokenStorage)
    {
        _httpClient = httpClient;
        _tokenStorage = tokenStorage;
    }

    public async Task<Result<AddProjectResponse>> AddAsync(
        AddProjectRequest request,
        CancellationToken cancellationToken = default
    )
    {
        AppendAuthorizationHeader();
        using var response = await _httpClient.PostAsJsonAsync(
            "api/projects",
            request,
            cancellationToken
        );

        var result = await response.Content.ReadFromJsonAsync<AddProjectResponse>(
            cancellationToken: cancellationToken
        );
        return Result<AddProjectResponse>.Success(result);
    }

    public async Task<Result<bool>> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        AppendAuthorizationHeader();
        using var response = await _httpClient.DeleteAsync($"api/projects/{id}", cancellationToken);

        return Result<bool>.Success(true);
    }

    public async Task<Result<UpdateProjectResponse>> UpdateAsync(
        int id,
        UpdateProjectRequest request,
        CancellationToken cancellationToken = default
    )
    {
        AppendAuthorizationHeader();
        using var response = await _httpClient.PatchAsJsonAsync(
            $"api/projects/{id}",
            request,
            cancellationToken
        );

        if (response.StatusCode == HttpStatusCode.NotFound)
            return Result<UpdateProjectResponse>.Failure(ProjectsErrors.ProjectNotFound);

        var result = await response.Content.ReadFromJsonAsync<UpdateProjectResponse>(
            cancellationToken: cancellationToken
        );
        return Result<UpdateProjectResponse>.Success(result);
    }

    public async Task<Result<List<GetProjectsResponse>>> GetProjectsAsync(
        int page,
        int pageSize,
        string? searchFilter = "",
        CancellationToken cancellationToken = default
    )
    {
        AppendAuthorizationHeader();
        var search = Uri.EscapeDataString(searchFilter ?? string.Empty);
        var url = $"api/projects?page={page}&pageSize={pageSize}&searchFilter={search}";

        using var response = await _httpClient.GetAsync(url, cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<List<GetProjectsResponse>>(
            cancellationToken: cancellationToken
        );
        return Result<List<GetProjectsResponse>>.Success(result);
    }

    public async Task<Result<GetProjectsCountResponse>> GetCountAsync(
        CancellationToken cancellationToken = default
    )
    {
        AppendAuthorizationHeader();
        using var response = await _httpClient.GetAsync("api/projects/count", cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<GetProjectsCountResponse>(
            cancellationToken: cancellationToken
        );
        return Result<GetProjectsCountResponse>.Success(result);
    }

    public async Task<Result<GetProjectsResponse>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        AppendAuthorizationHeader();
        using var response = await _httpClient.GetAsync($"api/projects/{id}", cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<GetProjectsResponse>(cancellationToken: cancellationToken);
        return Result<GetProjectsResponse>.Success(result);
    }

    private void AppendAuthorizationHeader()
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;

        if (_tokenStorage.IsAuthenticated)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenStorage.AccessToken);
        }
    }
}
