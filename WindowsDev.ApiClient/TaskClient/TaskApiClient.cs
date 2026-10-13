using System.Net.Http.Json;
using WindowsDev.Application.Identity.Cryptography;
using WindowsDev.Application.Primitives;
using WindowsDev.Contracts.Api.Common;
using WindowsDev.Contracts.Api.Tasks;
using TaskStatus = WindowsDev.Domain.Enums.TaskStatus;

namespace WindowsDev.ApiClient.TaskClient;

public class TaskApiClient : ITaskApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ISecureTokenStorage _tokenStorage;

    public TaskApiClient(HttpClient httpClient, ISecureTokenStorage secureTokenStorage)
    {
        _httpClient = httpClient;
        _tokenStorage = secureTokenStorage;
    }

    public async Task<AddTaskResponse> AddAsync(
        AddTaskRequest request,
        CancellationToken cancellationToken = default
    )
    {
        AppendAuthorizationHeader();
        using var response = await _httpClient.PostAsJsonAsync(
            "api/tasks",
            request,
            cancellationToken
        );

        var result = await response.Content.ReadFromJsonAsync<AddTaskResponse>(
            cancellationToken: cancellationToken
        );
        return result;
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        AppendAuthorizationHeader();
        using var response = await _httpClient.DeleteAsync($"api/tasks/{id}", cancellationToken);
        return Result<bool>.Success(true);
    }

    public async Task<Result<UpdateTaskResponse>> UpdateAsync(
        int id,
        UpdateTaskRequest request,
        CancellationToken cancellationToken = default
    )
    {
        AppendAuthorizationHeader();
        using var response = await _httpClient.PatchAsJsonAsync(
            $"api/tasks/{id}",
            request,
            cancellationToken
        );

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<UpdateTaskResponse>(
                cancellationToken: cancellationToken
            );
            return Result<UpdateTaskResponse>.Success(result);
        }

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        var errorCode = problem?.GetErrorCode() ?? string.Empty;
        return Result<UpdateTaskResponse>.Failure(errorCode);
    }

    public async Task<Result<List<GetTasksResponse>>> GetTasksAsync(
        int projectId,
        string? seacrh,
        int page,
        int pageSize,
        List<TaskStatus> statuses,
        CancellationToken cancellationToken = default
    )
    {
        AppendAuthorizationHeader();
        var statusParams = string.Join("", statuses.Select(s => $"&statuses={(int)s}"));

        using var response = await _httpClient.GetAsync(
            $"api/tasks?projectId={projectId}&search={seacrh}&page={page}&pageSize={pageSize}{statusParams}"
        );
        var result = await response.Content.ReadFromJsonAsync<List<GetTasksResponse>>(
            cancellationToken: cancellationToken
        );
        return Result<List<GetTasksResponse>>.Success(result);
    }

    public async Task<Result<GetTasksCountResponse>> GetTasksCountAsync(
        int projectId,
        CancellationToken cancellationToken = default
    )
    {
        AppendAuthorizationHeader();
        var response = await _httpClient.GetAsync($"api/tasks/count?projectId={projectId}");
        var result = await response.Content.ReadFromJsonAsync<GetTasksCountResponse>(
            cancellationToken: cancellationToken
        );
        return Result<GetTasksCountResponse>.Success(result);
    }

    public async Task<Result<GetTaskByIdResponse>> GetTaskAsync(
        int id,
        CancellationToken cancellationToken
    )
    {
        AppendAuthorizationHeader();
        var response = await _httpClient.GetAsync($"api/tasks/{id}", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<GetTaskByIdResponse>(
                cancellationToken: cancellationToken
            );
            return Result<GetTaskByIdResponse>.Success(result);
        }

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(
            cancellationToken: cancellationToken
        );
        var error = problem?.GetErrorCode() ?? string.Empty;
        return Result<GetTaskByIdResponse>.Failure(error);
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
