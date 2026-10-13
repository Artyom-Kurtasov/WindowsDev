using System.ComponentModel.DataAnnotations;

namespace WindowsDev.Contracts.Api.Attachments
{
    public class AddAttachmentResponse
    {
        [Required]
        public required string FileExtension { get; set; }
        [Required]
        public required string FilePath { get; set; }
        [Required]
        public required string FileName { get; set; }
        public required long FileSize { get; set; }
        public required int TaskId { get; set; }
    }
}
