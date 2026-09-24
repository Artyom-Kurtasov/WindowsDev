using WindowsDev.Application.Primitives;
using WindowsDev.Domain.Entities;

namespace WindowsDev.Application.Tasks.Attachment;

public interface IAttachmentService
{
    Task<Result<List<TaskAttachment>>> GetAttachmentsAsync(int taskId);
    Task<Result<TaskAttachment>> AddFile(int taskId);
    Task OpenFile(string filePath);
    Task<Result<TaskAttachment>> GetAttachmentAsync(int attachmentId);
}