using WindowsDev.Domain.Entities;

namespace WindowsDev.Application.Tasks.Attachment;

public interface IAttachmentRepository
{
    Task<List<TaskAttachment>> GetAttachmentsAsync(int taskId);
    Task AddFileInfoToDatabase(TaskAttachment attachment);
    Task<TaskAttachment?> GetAttachmentAsync(int attachmentId);
}