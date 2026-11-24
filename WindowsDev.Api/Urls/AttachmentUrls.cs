namespace WindowsDev.Api.Urls;

public static class AttachmentUrls
{
    public const string BaseAddress = "api/attachments";
    public const string DownloadFileUrl = "download";
    public const string UploadFileUrl = "{taskId:int}";
    public const string GetFilesUrl = "{taskId:int}";
}