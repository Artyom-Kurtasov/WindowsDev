using WindowsDev.Domain.TasksModels;

namespace WindowsDev.Business.Services.TaskService.Comment.Interfaces
{
    public interface ICommentService
    {
        Task<List<Comments>> GetComments(int taskId);
        Task<Comments> AddComment(int taskId, string commentText);
    }
}
