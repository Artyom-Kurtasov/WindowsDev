namespace WindowsDev.Api.DTO.Response.AttachmentsController;

public class GetAttachmentResponse
{
    public int Id { get; set; }
    public required string FileExtension { get; set; }
    public required string FilePath { get; set; }
    public required string FileName { get; set; }
    public required long FileSize { get; set; }
    public required int TaskId { get; set; }
}
