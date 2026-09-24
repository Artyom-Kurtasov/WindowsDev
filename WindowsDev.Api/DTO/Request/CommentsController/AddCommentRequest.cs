using System.ComponentModel.DataAnnotations;

namespace WindowsDev.Api.DTO.Request.CommentsController;

public class AddCommentRequest
{
    [Required]
    public int TaskId { get; set; }
    [Required]
    public string CommentText { get; set; } = string.Empty;
}
