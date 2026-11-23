using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using WindowsDev.Application.Identity.Cryptography;
using WindowsDev.Application.Primitives;
using WindowsDev.Contracts.Api.Common;
using WindowsDev.Contracts.Api.Tasks;
using WindowsDev.Contracts.UI.Tasks;
using TaskStatus = WindowsDev.Domain.Enums.TaskStatus;

namespace WindowsDev.ApiClient.TaskClient;

public class TaskApiClient : ApiClientBase, ITaskApiClient
{
    private readonly ILogger<TaskApiClient> _logger;
    private readonly IMapper _mapper;

    public TaskApiClient(HttpClient httpClient, ISecureTokenStorage tokenStorage, IMapper mapper, ILogger<TaskApiClient> logger)
        : base(httpClient, tokenStorage)
    {
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<TaskInfo>> AddAsync(
        TaskForm request,
        CancellationToken cancellationToken = default
    )
    {
        await AppendAuthorizationHeaderAsync(cancellationToken);
        var apiRequest = _mapper.Map<AddTaskRequest>(request);
        using var response = await _httpClient.PostAsJsonAsync(
            "api/tasks",
            apiRequest,
            cancellationToken
        );

        var result = await response.Content.ReadFromJsonAsync<AddTaskResponse>(
            cancellationToken: cancellationToken
        );

        var taskInfo = _mapper.Map<TaskInfo>(result);
        return Result<TaskInfo>.Success(taskInfo);
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        await AppendAuthorizationHeaderAsync(cancellationToken);
        using var response = await _httpClient.DeleteAsync($"api/tasks/{id}", cancellationToken);
        return Result<bool>.Success(true);
    }

    public async Task<Result<UpdateTaskInfo>> UpdateAsync(
        int id,
        UpdateTaskForm form,
        CancellationToken cancellationToken = default
    )
    {
        await AppendAuthorizationHeaderAsync(cancellationToken);
        var request = _mapper.Map<UpdateTaskRequest>(form);
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

            var info = _mapper.Map<UpdateTaskInfo>(result);
            return Result<UpdateTaskInfo>.Success(info);
        }

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        var errorCode = problem?.GetErrorCode() ?? string.Empty;

        return Result<UpdateTaskInfo>.Failure(errorCode);
    }

    public async Task<Result<List<TaskInfo>>> GetTasksAsync(
        int projectId,
        string? seacrh,
        int page,
        int pageSize,
        List<TaskStatus> statuses,
        CancellationToken cancellationToken = default
    )
    {
        await AppendAuthorizationHeaderAsync(cancellationToken);
        var statusParams = string.Join("", statuses.Select(s => $"&statuses={(int)s}"));

        using var response = await _httpClient.GetAsync(
            $"api/tasks?projectId={projectId}&search={seacrh}&page={page}&pageSize={pageSize}{statusParams}"
        );
        var result = await response.Content.ReadFromJsonAsync<List<GetTasksResponse>>(
            cancellationToken: cancellationToken
        );

        var info = _mapper.Map<List<TaskInfo>>(result);
        return Result<List<TaskInfo>>.Success(info);
    }

    public async Task<Result<int>> GetTasksCountAsync(
        int projectId,
        CancellationToken cancellationToken = default
    )
    {
        await AppendAuthorizationHeaderAsync(cancellationToken);
        var response = await _httpClient.GetAsync($"api/tasks/count?projectId={projectId}");

        var result = await response.Content.ReadFromJsonAsync<GetTasksCountResponse>(
            cancellationToken: cancellationToken
        );

        return Result<int>.Success(result.TotalCount);
    }

    public async Task<Result<TaskInfo>> GetTaskAsync(int id, CancellationToken cancellationToken)
    {
        await AppendAuthorizationHeaderAsync(cancellationToken);
        var response = await _httpClient.GetAsync($"api/tasks/{id}", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<GetTaskByIdResponse>(
                cancellationToken: cancellationToken
            );

            var info = _mapper.Map<TaskInfo>(result);
            return Result<TaskInfo>.Success(info);
        }

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(
            cancellationToken: cancellationToken
        );
        var error = problem?.GetErrorCode() ?? string.Empty;
        return Result<TaskInfo>.Failure(error);
    }
}
