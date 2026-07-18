using Microsoft.Extensions.Logging;

namespace WindowsDev.Infrastructure.Logging
{
    public static partial class AttachmentLogs
    {
        [LoggerMessage(
            EventId = EventIds.AttachmentUploadFailed,
            Level = LogLevel.Error,
            Message = "Failed to upload attachment '{FileName}' ({FileSize} bytes)")]
        internal static partial void AttachmentUploadFailed(
            ILogger logger,
            string fileName,
            long fileSize,
            Exception exception);

        [LoggerMessage(
            EventId = EventIds.AttachmentDeleteFailed,
            Level = LogLevel.Error,
            Message = "Failed to delete attachment {AttachmentId}")]
        internal static partial void AttachmentDeleteFailed(
            ILogger logger,
            string attachmentId,
            Exception exception);

        [LoggerMessage(
            EventId = EventIds.AttachmentLoadFailed,
            Level = LogLevel.Error,
            Message = "Failed to load attachments for task {TaskId}")]
        internal static partial void AttachmentLoadFailed(
            ILogger logger,
            string taskId,
            Exception exception);

        [LoggerMessage(
            EventId = EventIds.AttachmentOpenFailed,
            Level = LogLevel.Error,
            Message = "Failed to download attachment {AttachmentId}: '{FileName}'")]
        internal static partial void AttachmentOpenFailed(
            ILogger logger,
            string attachmentId,
            string fileName,
            Exception exception);

        [LoggerMessage(
            EventId = EventIds.AttachmentNotFound,
            Level = LogLevel.Warning,
            Message = "Attachment {AttachmentId} not found")]
        internal static partial void AttachmentNotFound(
            ILogger logger,
            string attachmentId);

        [LoggerMessage(
            EventId = EventIds.AttachmentStorageCritical,
            Level = LogLevel.Critical,
            Message = "Attachment storage unavailable. All file operations will fail.")]
        internal static partial void AttachmentStorageCritical(
            ILogger logger,
            Exception exception);
    }
}