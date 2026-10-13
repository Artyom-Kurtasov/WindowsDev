using System.ComponentModel.DataAnnotations;

namespace WindowsDev.Contracts.Api.Attachments;

public class OpenAttachmentRequest
{
    [Required]
    public string FilePath { get; set; } = string.Empty;
}
