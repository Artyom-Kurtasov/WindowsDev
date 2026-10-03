using WindowsDev.Application.Primitives;
using WindowsDev.Domain.Entities;

namespace WindowsDev.Application.Tasks.Comment;

public interface ICommentService
{
    Task<List<TaskComment>> GetCommentsAsync(int taskId);
    Task<TaskComment> AddCommentAsync(int taskId, string commentText);
    Task<Result<TaskComment>> GetCommentAsync(int id);
}