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

        public async Task<TaskAttachment?> AddFile(string filePath, int taskId)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Filepath is empty", nameof(filePath));
            if (taskId < 1)
                throw new ArgumentException("Invalid task id", nameof(taskId));

            FileInfo fileInfo = new FileInfo(filePath);

            if (!fileInfo.Exists)
                throw new FileNotFoundException("File not found", filePath);

            TaskAttachment attachment = new TaskAttachment
            {
                FileName = fileInfo.Name,
                FilePath = filePath,
                FileExtension = fileInfo.Extension,
                FileSize = fileInfo.Length,
                TaskId = taskId
            };

            await _attachmentRepository.AddFileInfoToDatabase(attachment);

            return attachment;
        }
    }
}
