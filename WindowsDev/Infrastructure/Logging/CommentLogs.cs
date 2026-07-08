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
            EventId = EventIds.CommentDeleteFailed,
            Level = LogLevel.Error,
            Message = "Failed to delete comment {CommentId} from task {TaskId}")]
        internal static partial void CommentDeleteFailed(
            ILogger logger,
            string commentId,
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

        [LoggerMessage(
            EventId = EventIds.CommentEmpty,
            Level = LogLevel.Warning,
            Message = "Attempted to add empty comment to task {TaskId}")]
        internal static partial void CommentEmpty(
            ILogger logger,
            string taskId);

        [LoggerMessage(
            EventId = EventIds.CommentNotFound,
            Level = LogLevel.Warning,
            Message = "Comment {CommentId} not found")]
        internal static partial void CommentNotFound(
            ILogger logger,
            string commentId);
    }
}