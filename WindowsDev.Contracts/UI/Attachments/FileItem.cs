namespace WindowsDev.Contracts.UI.Attachments;

public record FileItem(
    string FileName,
    long Size,
    DateTime LastModifiedUtc
);
