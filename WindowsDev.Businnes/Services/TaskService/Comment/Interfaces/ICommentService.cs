using WindowsDev.Business.Primitives;
using WindowsDev.Domain.TasksModels;

namespace WindowsDev.Business.Services.TaskService.Comment.Interfaces
{
    public interface ICommentService
    {
        Task<List<Comments>> GetComments(int taskId);
        Task<Result<Comments>> AddComment(int taskId, string commentText);
    }
}
