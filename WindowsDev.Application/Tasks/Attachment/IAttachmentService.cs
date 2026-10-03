using WindowsDev.Application.Primitives;
using WindowsDev.Domain.Entities;

namespace WindowsDev.Application.Tasks.Attachment;

public interface IAttachmentService
{
    Task<List<TaskAttachment>> GetAttachmentsAsync(int taskId);
    Task<Result<TaskAttachment>> AddFile(string fileName, int taskId);
    Task OpenFile(string filePath);
    Task<Result<TaskAttachment>> GetAttachmentAsync(int attachmentId);
}