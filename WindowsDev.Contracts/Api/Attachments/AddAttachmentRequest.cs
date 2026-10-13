namespace WindowsDev.Contracts.Api.Attachments
{
    public class AddAttachmentRequest
    {
        public required string FileName { get; set; } = string.Empty;
        public required int TaskId { get; set; }
    }
}
