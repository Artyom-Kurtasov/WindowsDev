using System.ComponentModel.DataAnnotations;

namespace WindowsDev.Contracts.Api.Comments;

public class AddCommentRequest
{
    [Required]
    public int TaskId { get; set; }
    [Required]
    public string Text { get; set; } = string.Empty;
}
