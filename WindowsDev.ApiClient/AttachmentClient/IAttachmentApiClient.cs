using WindowsDev.Application.Primitives;
using WindowsDev.Contracts.UI.Tasks;

namespace WindowsDev.ApiClient.AttachmentClient;

public interface IAttachmentApiClient
{
    Task<Result<AttachmentInfo>> AddFileAsync(
        AttachmentForm request,
        CancellationToken cancellationToken = default
    );

    Task<Result<List<AttachmentInfo>>> GetAttachmentsAsync(
        int taskId,
        CancellationToken cancellationToken = default
    );

    Task OpenAttachmentAsync(
        string filePath,
        CancellationToken cancellationToken = default
    );

    Task<Result<AttachmentInfo>> GetAttachmentAsync(
        int id,
        CancellationToken cancellationToken = default
    );
}
