using WindowsDev.Business.Primitives;
using WindowsDev.Domain.TasksModels;

namespace WindowsDev.Business.Services.TaskService.Attachment.Interfaces
{
    public interface IAttacmentService
    {
        Task<List<TaskAttachment>> GetAttachmentsAsync(int taskId);
        Task<Result<TaskAttachment>> AddFile(int taskId);
        Task OpenFile(TaskAttachment attachment);
    }
}
