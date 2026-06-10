using WindowsDev.Domain.TasksModels;

namespace WindowsDev.Business.Repositories.Interfaces
{
    public interface ICommentRepository
    {
        Task AddComments(Comments comment);
        Task<List<Comments>> GetComments(int taskId);
    }
}
