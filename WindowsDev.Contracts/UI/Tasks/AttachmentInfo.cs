namespace WindowsDev.Contracts.UI.Tasks;

public record AttachmentInfo(
    string FileExtension,
    string FilePath,
    string FileName,
    long FileSize,
    int TaskId);
