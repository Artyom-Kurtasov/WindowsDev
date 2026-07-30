using WindowsDev.Domain.Entities;

namespace WindowsDev.Application.RepositoriesInterfaces
{
    public interface IAttachmentRepository
    {
        Task<List<TaskAttachment>> GetAttachmentsAsync(int taskId);
        Task AddFileInfoToDatabase(TaskAttachment attachment);
    }
}
