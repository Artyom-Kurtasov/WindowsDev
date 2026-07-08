using Microsoft.Extensions.Logging;

namespace WindowsDev.Infrastructure.Logging
{
    public static partial class TaskLogs
    {
        [LoggerMessage(
            EventId = EventIds.TaskCreationFailed,
            Level = LogLevel.Error,
            Message = "Failed to create task '{TaskName}' in project {ProjectId}")]
        public static partial void TaskCreationFailed(
            ILogger logger,
            string taskName,
            int projectId,
            Exception exception);

        [LoggerMessage(
            EventId = EventIds.TaskDeleteFailed,
            Level = LogLevel.Error,
            Message = "Failed to delete task {TaskId}")]
        public static partial void TaskDeleteFailed(
            ILogger logger,
            int taskId,
            Exception exception);

        [LoggerMessage(
            EventId = EventIds.TaskLoadFailed,
            Level = LogLevel.Error,
            Message = "Failed to load tasks for project {ProjectId}")]
        public static partial void TaskLoadFailed(
            ILogger logger,
            int projectId,
            Exception exception);

        [LoggerMessage(
            EventId = EventIds.TaskUpdateFailed,
            Level = LogLevel.Error,
            Message = "Failed to update task {TaskId}")]
        public static partial void TaskUpdateFailed(
            ILogger logger,
            int taskId,
            Exception exception);

        [LoggerMessage(
            EventId = EventIds.TaskNotFound,
            Level = LogLevel.Warning,
            Message = "Task {TaskId} not found")]
        public static partial void TaskNotFound(
            ILogger logger,
            int taskId);
    }
}