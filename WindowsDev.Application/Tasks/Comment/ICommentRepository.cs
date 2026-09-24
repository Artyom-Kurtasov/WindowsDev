using WindowsDev.Domain.Entities;

namespace WindowsDev.Application.Tasks.Comment;

public interface ICommentRepository
{
    Task AddComments(TaskComment comment);
    Task<List<TaskComment>> GetComments(int taskId);
    Task<TaskComment?> GetComment(int id);
}