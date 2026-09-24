using System.ComponentModel.DataAnnotations;

namespace WindowsDev.Api.DTO.Request.AttachmentsController;

public class OpenAttachmentRequest
{
    [Required]
    public string FilePath { get; set; } = string.Empty;
}
