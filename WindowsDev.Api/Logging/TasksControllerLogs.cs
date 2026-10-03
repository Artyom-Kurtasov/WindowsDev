using WindowsDev.Api.Logging.EventId;

namespace WindowsDev.Api.Logging;

internal static partial class TasksControllerLogs
{
    [LoggerMessage(
        EventId = TasksControllerEventId.AddSuccessful,
        Level = LogLevel.Information,
        Message = "Task was add successful. Task id: {taskId}, project id: {projectId}"
    )]
    public static partial void AddSuccessful(ILogger logger, int taskId, int projectId);

    [LoggerMessage(
        EventId = TasksControllerEventId.DeleteSuccessful,
        Level = LogLevel.Information,
        Message = "Task was delete successful. Task id: {taskId}"
    )]
    public static partial void DeleteSuccessful(ILogger logger, int taskId);

    [LoggerMessage(
        EventId = TasksControllerEventId.UpdateSuccessful,
        Level = LogLevel.Information,
        Message = "Task was update successful. Task id: {taskId}"
    )]
    public static partial void UpdateSuccessful(ILogger logger, int taskId);

    [LoggerMessage(
        EventId = TasksControllerEventId.TaskNotFound,
        Level = LogLevel.Warning,
        Message = "Task with id {taskId} not found"
    )]
    public static partial void TaskNotFound(ILogger logger, int taskId);

    [LoggerMessage(
        EventId = TasksControllerEventId.GetTasksSuccessful,
        Level = LogLevel.Information,
        Message = "The tasks were received successfully. Project id: {projectId}, page: {page}, page size: {pageSize}, search filter: {searchFilter}"
    )]
    public static partial void GetTasksSuccessful(
        ILogger logger,
        int projectId,
        int page,
        int pageSize,
        string? searchFilter
    );

    [LoggerMessage(
        EventId = TasksControllerEventId.GetTasksCountSuccessful,
        Level = LogLevel.Information,
        Message = "The tasks count were received successfuly. Project id: {projectId}"
    )]
    public static partial void GetTasksCountSuccessful(ILogger logger, int projectId);

    [LoggerMessage(
        EventId = TasksControllerEventId.GetTaskSuccessful,
        Level = LogLevel.Information,
        Message = "The task was received successfully. Task id: {taskId}"
    )]
    public static partial void GetTaskSuccessful(ILogger logger, int taskId);
}
