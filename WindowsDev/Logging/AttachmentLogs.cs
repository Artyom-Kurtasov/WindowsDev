using Microsoft.Extensions.Logging;

namespace WindowsDev.Logging;

internal static partial class AttachmentLogs
{
    [LoggerMessage(
        EventId = EventIds.AttachmentUploadFailed,
        Level = LogLevel.Error,
        Message = "Failed to upload attachment '{FileName}' ({FileSize} bytes)")]
    public static partial void AttachmentUploadFailed(
        ILogger logger,
        string fileName,
        long fileSize,
        Exception exception);

    [LoggerMessage(
        EventId = EventIds.AttachmentLoadFailed,
        Level = LogLevel.Error,
        Message = "Failed to load attachments for task {TaskId}")]
    public static partial void AttachmentLoadFailed(
        ILogger logger,
        string taskId,
        Exception exception);

}
