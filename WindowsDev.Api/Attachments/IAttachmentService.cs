using WindowsDev.Application.Primitives;
using WindowsDev.Contracts.Api.Attachments;

namespace WindowsDev.Api.Attachments;

public interface IAttachmentService
{
    Task<Result<string>> UploadFileAsync(IFormFile file);
    Result<DownloadFileResponse> DownloadFile(string fileName);
    Result<List<FileListItemResponse>> GetFiles();
}
