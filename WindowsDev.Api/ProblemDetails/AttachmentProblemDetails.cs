namespace WindowsDev.Api.ProblemDetails;

public static class AttachmentProblemDetails
{
    public const string AttachmentNotFound = "Attachment Not Found";
    public static string AttachmentNotFoundDeatil(int id) => $"Attachment with id {id} not found.";
    public const string FileDidNotSelect = "File Did Not Select";
    public const string FileDidNotSelectDetail = "File Did Not Select";
}
