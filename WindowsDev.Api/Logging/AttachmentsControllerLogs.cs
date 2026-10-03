using WindowsDev.Api.Logging.EventId;

namespace WindowsDev.Api.Logging;

internal static partial class AttachmentsControllerLogs
{
    [LoggerMessage(
        EventId = AttachmentsControllerEventId.AddFileSuccessful,
        Level = LogLevel.Information,
        Message = "Attachment was add successful. Attachment id: {attachmentId}, task id: {taskId}"
    )]
    public static partial void AddFileSuccessful(ILogger logger, int attachmentId, int taskId);

    [LoggerMessage(
        EventId = AttachmentsControllerEventId.FileNotSelected,
        Level = LogLevel.Warning,
        Message = "File was not selected for task id {taskId}"
    )]
    public static partial void FileNotSelected(ILogger logger, int taskId);

    [LoggerMessage(
        EventId = AttachmentsControllerEventId.GetAttachmentsSuccessful,
        Level = LogLevel.Information,
        Message = "The attachments were received successfully. Task id: {taskId}"
    )]
    public static partial void GetAttachmentsSuccessful(ILogger logger, int taskId);

    [LoggerMessage(
        EventId = AttachmentsControllerEventId.OpenAttachmentSuccessful,
        Level = LogLevel.Information,
        Message = "Attachment was opened successfully. File path: {filePath}"
    )]
    public static partial void OpenAttachmentSuccessful(ILogger logger, string filePath);

    [LoggerMessage(
        EventId = AttachmentsControllerEventId.GetAttachmentSuccessful,
        Level = LogLevel.Information,
        Message = "The attachment was received successfully. Attachment id: {attachmentId}"
    )]
    public static partial void GetAttachmentSuccessful(ILogger logger, int attachmentId);

    [LoggerMessage(
        EventId = AttachmentsControllerEventId.AttachmentNotFound,
        Level = LogLevel.Warning,
        Message = "Attachment with id {attachmentId} not found"
    )]
    public static partial void AttachmentNotFound(ILogger logger, int attachmentId);
}
