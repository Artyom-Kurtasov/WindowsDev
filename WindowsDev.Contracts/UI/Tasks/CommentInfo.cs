namespace WindowsDev.Contracts.UI.Tasks;

public record CommentInfo(
    int Id,
    int TaskId,
    string Text,
    string Author,
    DateTime CreatedAt);
