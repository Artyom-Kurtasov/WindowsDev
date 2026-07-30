using WindowsDev.Application.Primitives;
using WindowsDev.Domain.Entities;

namespace WindowsDev.Application.Services.TaskService.Attachment
{
    public interface IAttacmentService
    {
        Task<List<TaskAttachment>> GetAttachmentsAsync(int taskId);
        Task<Result<TaskAttachment>> AddFile(int taskId);
        Task OpenFile(TaskAttachment attachment);
    }
}
