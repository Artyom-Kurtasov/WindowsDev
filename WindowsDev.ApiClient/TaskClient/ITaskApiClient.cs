using WindowsDev.Application.Primitives;
using WindowsDev.Contracts.Api.Tasks;
using TaskStatus = WindowsDev.Domain.Enums.TaskStatus;

namespace WindowsDev.ApiClient.TaskClient;

public interface ITaskApiClient
{
    Task<AddTaskResponse> AddAsync(
        AddTaskRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<UpdateTaskResponse>> UpdateAsync(
        int id,
        UpdateTaskRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<List<GetTasksResponse>>> GetTasksAsync(
        int projectId,
        string? seacrh,
        int page,
        int pageSize,
        List<TaskStatus> statuses,
        CancellationToken cancellationToken = default
    );

    Task<Result<GetTasksCountResponse>> GetTasksCountAsync(
        int projectId,
        CancellationToken cancellationToken = default
    );

    Task<Result<GetTaskByIdResponse>> GetTaskAsync(
        int id,
        CancellationToken cancellationToken = default
    );
}
