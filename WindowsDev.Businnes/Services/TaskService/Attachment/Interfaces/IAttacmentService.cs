using WindowsDev.Domain.TasksModels;

namespace WindowsDev.Business.Services.TaskService.Attachment.Interfaces
{
    public interface IAttacmentService
    {
        Task<List<TaskAttachment>> GetAttachmentsAsync(int taskId);
        Task<TaskAttachment?> AddFile(string filePath, int taskId);
    }
}
