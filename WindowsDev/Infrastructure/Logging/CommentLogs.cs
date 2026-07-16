using Microsoft.Extensions.Logging;

namespace WindowsDev.Infrastructure.Logging
{
    public static partial class CommentLogs
    {
        [LoggerMessage(
            EventId = EventIds.CommentCreationFailed,
            Level = LogLevel.Error,
            Message = "Failed to add comment to task {TaskId}")]
        internal static partial void CommentCreationFailed(
            ILogger logger,
            string taskId,
            Exception exception);

        [LoggerMessage(
            EventId = EventIds.CommentLoadFailed,
            Level = LogLevel.Error,
            Message = "Failed to load comments for task {TaskId}")]
        internal static partial void CommentLoadFailed(
            ILogger logger,
            string taskId,
            Exception exception);
    }
}