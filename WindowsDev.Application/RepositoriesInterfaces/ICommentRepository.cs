using WindowsDev.Domain.Entities;

namespace WindowsDev.Application.RepositoriesInterfaces
{
    public interface ICommentRepository
    {
        Task AddComments(TaskComment comment);
        Task<List<TaskComment>> GetComments(int taskId);
    }
}
