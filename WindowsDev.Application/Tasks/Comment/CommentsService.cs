using WindowsDev.Application.Identity;
using WindowsDev.Application.Primitives;
using WindowsDev.Domain.Entities;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;

namespace WindowsDev.Application.Tasks.Comment;

internal class CommentsService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly IUserSession _userSession;

    public CommentsService(
        ICommentRepository commentRepository,
        IUserSession userSession
    )
    {
        _commentRepository = commentRepository;
        _userSession = userSession;
    }

    public async Task<TaskComment> AddCommentAsync(int taskId, string commentText)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(taskId);

        var comment = new TaskComment
        {
            Text = commentText,
            CreatedAt = DateTime.UtcNow,
            Author = _userSession.Username,
            TaskId = taskId,
        };

        await _commentRepository.AddComments(comment);

        return comment;
    }

    public async Task<List<TaskComment>> GetCommentsAsync(int taskId) =>
        await _commentRepository.GetComments(taskId);

    public async Task<Result<TaskComment>> GetCommentAsync(int id)
    {
        var comment = await _commentRepository.GetComment(id);

        if (comment == null)
        {
            return Result<TaskComment>.Failure(CommentErrors.CommentNotFound);
        }

        return Result<TaskComment>.Success(comment);
    }

}