using WindowsDev.Api.DTO.Request.AttachmentsController;
using WindowsDev.Api.DTO.Response.AttachmentsController;
using WindowsDev.Application.Primitives;

namespace WindowsDev.ApiClients.AttachmentClient;

public interface IAttachmentApiClient
{
    Task<Result<AddFileResponse>> AddFileAsync(
        AddFileRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<List<GetAttachmentResponse>>> GetAttachmentsAsync(
        int taskId,
        CancellationToken cancellationToken = default
    );

    Task OpenAttachmentAsync(
        string filePath,
        CancellationToken cancellationToken = default
    );

    Task<Result<GetAttachmentResponse>> GetAttachmentAsync(
        int id,
        CancellationToken cancellationToken = default
    );
}
