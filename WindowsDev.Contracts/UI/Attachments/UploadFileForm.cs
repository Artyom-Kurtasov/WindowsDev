namespace WindowsDev.Contracts.UI.Attachments;

public record UploadFileForm(
    string FileName,
    string ContentType,
    Stream Content
);
