using WindowsDev.Business.Primitives;
using WindowsDev.Business.Repositories.Interfaces;
using WindowsDev.Business.Services.TaskService.Attachment.Interfaces;
using WindowsDev.Domain.TasksModels;

namespace WindowsDev.Business.Services.TaskService.Attachment
{
    public class AttachmentService : IAttacmentService
    {
        private readonly IAttachmentRepository _attachmentRepository;

        public AttachmentService(IAttachmentRepository attachmentRepository)
        {
            _attachmentRepository = attachmentRepository;
        }

        public async Task<List<TaskAttachment>> GetAttachmentsAsync(int taskId)
        {
            return await _attachmentRepository.GetAttachmentsAsync(taskId);
        }

        public async Task<Result<TaskAttachment>> AddFile(string filePath, int taskId)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(taskId);

            FileInfo fileInfo = new FileInfo(filePath);

            TaskAttachment attachment = new TaskAttachment
            {
                FileName = fileInfo.Name,
                FilePath = filePath,
                FileExtension = fileInfo.Extension,
                FileSize = fileInfo.Length,
                TaskId = taskId
            };

            await _attachmentRepository.AddFileInfoToDatabase(attachment);

            return Result<TaskAttachment>.Success(attachment);
        }
    }
}
