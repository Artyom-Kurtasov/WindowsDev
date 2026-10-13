namespace WindowsDev.Contracts.Api.Attachments;

public class GetAttachmentResponse
{
    public required string FileExtension { get; set; }
    public required string FilePath { get; set; }
    public required string FileName { get; set; }
    public required long FileSize { get; set; }
    public required int TaskId { get; set; }
}
