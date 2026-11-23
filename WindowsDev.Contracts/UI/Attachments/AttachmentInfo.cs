namespace WindowsDev.Contracts.UI.Attachments;

public record AttachmentInfo(
    string FileExtension,
    string FilePath,
    string FileName,
    long FileSize,
    int TaskId);
