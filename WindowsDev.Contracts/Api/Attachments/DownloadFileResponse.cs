namespace WindowsDev.Contracts.Api.Attachments;

public record DownloadFileResponse(Stream Stream, string ContentType, string FileName);