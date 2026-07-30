using WindowsDev.Application.Primitives;
using WindowsDev.Domain.Entities;

namespace WindowsDev.Application.Services.TaskService.Comment
{
    public interface ICommentService
    {
        Task<List<TaskComment>> GetComments(int taskId);
        Task<Result<TaskComment>> AddComment(int taskId, string commentText);
    }
}
