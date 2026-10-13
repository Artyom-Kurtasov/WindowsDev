using WindowsDev.Application.Primitives;
using WindowsDev.Contracts.Api.Comments;

namespace WindowsDev.ApiClient.CommentClient;

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
