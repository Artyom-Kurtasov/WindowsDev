using WindowsDev.Api.Logging.EventId;

namespace WindowsDev.Api.Logging;

internal static partial class AttachmentsControllerLogs
{
    [LoggerMessage(
        EventId = AttachmentsControllerEventId.UploadFileSuccessful,
        Level = LogLevel.Information,
        Message = "File was uploaded successfully. File name: {fileName}"
    )]
    public static partial void UploadFileSuccessful(ILogger logger, string fileName);

    [LoggerMessage(
        EventId = AttachmentsControllerEventId.FileNotSelected,
        Level = LogLevel.Warning,
        Message = "File was not selected for upload. File name: {fileName}"
    )]
    public static partial void FileNotSelected(ILogger logger, string fileName);

    [LoggerMessage(
        EventId = AttachmentsControllerEventId.GetFilesSuccessful,
        Level = LogLevel.Information,
        Message = "The file list was received successfully. Files count: {filesCount}"
    )]
    public static partial void GetFilesSuccessful(ILogger logger, int filesCount);

    [LoggerMessage(
        EventId = AttachmentsControllerEventId.GetFilesFailed,
        Level = LogLevel.Error,
        Message = "Failed to get the file list. Error: {error}"
    )]
    public static partial void GetFilesFailed(ILogger logger, string? error);

    [LoggerMessage(
        EventId = AttachmentsControllerEventId.DownloadFileSuccessful,
        Level = LogLevel.Information,
        Message = "File was downloaded successfully. File name: {fileName}"
    )]
    public static partial void DownloadFileSuccessful(ILogger logger, string fileName);

    [LoggerMessage(
        EventId = AttachmentsControllerEventId.AttachmentNotFound,
        Level = LogLevel.Warning,
        Message = "Attachment was not found. File name: {fileName}"
    )]
    public static partial void AttachmentNotFound(ILogger logger, string fileName);
}