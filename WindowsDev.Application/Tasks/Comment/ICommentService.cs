using WindowsDev.Application.Primitives;
using WindowsDev.Domain.Entities;

namespace WindowsDev.Application.Tasks.Comment;

public interface ICommentService
{
    Task<List<TaskComment>> GetCommentsAsync(int taskId);
    Task<Result<TaskComment>> AddCommentAsync(int taskId, string commentText);
    Task<TaskComment?> GetCommentAsync(int id);
}