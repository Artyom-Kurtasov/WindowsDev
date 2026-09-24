namespace WindowsDev.Api.DTO.Response.CommentsController;

public class GetCommentResponse
{
    public int Id { get; set; }
    public required string Text { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required string Author { get; set; }
    public required int TaskId { get; set; }
}
