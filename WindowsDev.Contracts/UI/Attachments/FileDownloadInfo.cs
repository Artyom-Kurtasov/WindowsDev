namespace WindowsDev.Contracts.UI.Attachments;

public record FileDownloadInfo(
    Stream Content,
    string ContentType,
    string FileName
);
