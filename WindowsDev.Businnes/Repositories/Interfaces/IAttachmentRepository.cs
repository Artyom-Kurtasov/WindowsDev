using WindowsDev.Domain.TasksModels;

namespace WindowsDev.Business.Repositories.Interfaces
{
    public interface IAttachmentRepository
    {
        Task<List<TaskAttachment>> GetAttachmentsAsync(int taskId);
        Task AddFileInfoToDatabase(TaskAttachment attachment);
    }
}
