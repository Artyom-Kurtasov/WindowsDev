using WindowsDev.Api.Logging.EventId;

namespace WindowsDev.Api.Logging;

internal static partial class CommentsControllerLogs
{
    [LoggerMessage(
        EventId = CommentsControllerEventId.GetCommentSuccessful,
        Level = LogLevel.Information,
        Message = "The comment was received successfully. Comment id: {commentId}"
    )]
    public static partial void GetCommentSuccessful(ILogger logger, int commentId);

    [LoggerMessage(
        EventId = CommentsControllerEventId.CommentNotFound,
        Level = LogLevel.Warning,
        Message = "Comment with id {commentId} not found"
    )]
    public static partial void CommentNotFound(ILogger logger, int commentId);

    [LoggerMessage(
        EventId = CommentsControllerEventId.GetCommentsSuccessful,
        Level = LogLevel.Information,
        Message = "The comments were received successfully. Task id: {taskId}"
    )]
    public static partial void GetCommentsSuccessful(ILogger logger, int taskId);

    [LoggerMessage(
        EventId = CommentsControllerEventId.AddCommentSuccessful,
        Level = LogLevel.Information,
        Message = "Comment was add successful. Comment id: {commentId}, task id: {taskId}"
    )]
    public static partial void AddCommentSuccessful(ILogger logger, int commentId, int taskId);
}
