using System.Diagnostics;
using Microsoft.Win32;
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

        public async Task<Result<TaskAttachment>> AddFile(int taskId)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(taskId);

            var dialog = new OpenFileDialog();

            if (dialog.ShowDialog() != true)
                return Result<TaskAttachment>.Failure(dialog.FileName);

            FileInfo fileInfo = new FileInfo(dialog.FileName);

            TaskAttachment attachment = new TaskAttachment
            {
                FileName = fileInfo.Name,
                FilePath = fileInfo.FullName,
                FileExtension = fileInfo.Extension,
                FileSize = fileInfo.Length,
                TaskId = taskId,
            };

            await _attachmentRepository.AddFileInfoToDatabase(attachment);

            return Result<TaskAttachment>.Success(attachment);
        }

        public async Task OpenFile(TaskAttachment attachment)
        {
            Process.Start(
                new ProcessStartInfo { FileName = attachment.FilePath, UseShellExecute = true }
            );
        }
    }
}
