using WindowsDev.Application.Primitives;
using WindowsDev.Contracts.UI.Comments;

namespace WindowsDev.ApiClient.CommentClient;

public interface ICommentApiClient
{
    Task<Result<CommentInfo>> GetCommentAsync(int id, CancellationToken cancellationToken);
    Task<Result<List<CommentInfo>>> GetCommentsAsync(
        int taskId,
        CancellationToken cancellationToken
    );
    Task<Result<CommentInfo>> AddCommentAsync(
        AddCommentForm request,
        CancellationToken cancellationToken
    );
}
