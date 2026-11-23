using WindowsDev.Application.Primitives;
using WindowsDev.Contracts.UI.Attachments;

namespace WindowsDev.ApiClient.AttachmentClient;

public interface IAttachmentApiClient
{
    Task<Result<string>> UploadFileAsync(
        UploadFileForm request,
        CancellationToken cancellationToken = default
    );

    Task<Result<List<FileItem>>> GetFilesAsync(CancellationToken cancellationToken = default);

    Task<Result<FileDownloadInfo>> DownloadFileAsync(
        string fileName,
        CancellationToken cancellationToken = default
    );
}
