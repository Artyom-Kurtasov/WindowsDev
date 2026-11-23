namespace WindowsDev.Contracts.Api.Attachments;

public record FileListItemResponse(string FileName, long Size, DateTime LastModifiedUtc);
