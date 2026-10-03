using WindowsDev.Api.DTO.Request.CommentsController;
using WindowsDev.Api.DTO.Response.CommentsController;
using WindowsDev.Application.Primitives;

namespace WindowsDev.ApiClients.CommentClient;

public interface ICommentApiClient
{
    Task<Result<GetCommentResponse>> GetCommentAsync(int id, CancellationToken cancellationToken);
    Task<Result<List<GetCommentResponse>>> GetCommentsAsync(
        int taskId,
        CancellationToken cancellationToken
    );
    Task<Result<AddCommentResponse>> AddCommentAsync(
        AddCommentRequest request,
        CancellationToken cancellationToken
    );
}
