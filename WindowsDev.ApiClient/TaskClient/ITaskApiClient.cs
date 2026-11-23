using WindowsDev.Application.Primitives;
using WindowsDev.Contracts.UI.Tasks;
using TaskStatus = WindowsDev.Domain.Enums.TaskStatus;

namespace WindowsDev.ApiClient.TaskClient;

public interface ITaskApiClient
{
    Task<Result<TaskInfo>> AddAsync(
        TaskForm request,
        CancellationToken cancellationToken = default
    );

    Task<Result<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<UpdateTaskInfo>> UpdateAsync(
        int id,
        UpdateTaskForm request,
        CancellationToken cancellationToken = default
    );

    Task<Result<List<TaskInfo>>> GetTasksAsync(
        int projectId,
        string? seacrh,
        int page,
        int pageSize,
        List<TaskStatus> statuses,
        CancellationToken cancellationToken = default
    );

    Task<Result<int>> GetTasksCountAsync(
        int projectId,
        CancellationToken cancellationToken = default
    );

    Task<Result<TaskInfo>> GetTaskAsync(
        int id,
        CancellationToken cancellationToken = default
    );
}
