namespace WindowsDev.Contracts.UI.Comments;

public record CommentInfo(
    int Id,
    string Text,
    DateTime CreatedAt,
    string Author,
    int TaskId);
