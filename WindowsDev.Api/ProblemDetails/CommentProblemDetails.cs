namespace WindowsDev.Api.ProblemDetails;

internal static class CommentProblemDetails
{
    public const string CommentNotFound = "Comment Not Found";
    public static string CommentNotFoundDetail(int id) => $"Comment with {id} not found.";
}
