namespace WindowsDev.Api.ProblemDetails;

public static class AttachmentProblemDetails
{
    public const string FileDidNotSelect = "File Did Not Select";
    public const string FileDidNotSelectDetail = "File Did Not Select";

    public const string AttachmentNotFound = "Attachment Not Found";

    public static string AttachmentNotFoundDetail(string fileName) =>
        $"Attachment with name '{fileName}' not found.";

    public const string UploadsFolderUnavailable = "Uploads Folder Unavailable";
    public const string UploadsFolderUnavailableDetail =
        "The uploads directory is not available on the server.";
}
