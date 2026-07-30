using WindowsDev.Application.Primitives;
using WindowsDev.Application.RepositoriesInterfaces;
using WindowsDev.Application.Services.TaskService.Attachment.FileService;
using WindowsDev.Application.Services.TaskService.Attachment.FileServiceInterfaces;
using WindowsDev.Domain.Common.DialogsMessages.Errors;
using WindowsDev.Domain.Entities;

namespace WindowsDev.Application.Services.TaskService.Attachment
{
    public class AttachmentService : IAttacmentService
    {
        private readonly IAttachmentRepository _attachmentRepository;
        private readonly IFilePicker _filePicker;
        private readonly IFileOpener _fileOpener;

        public AttachmentService(
            IAttachmentRepository attachmentRepository,
            IFilePicker filePicker,
            IFileOpener fileOpener)
        {
            _attachmentRepository = attachmentRepository;
            _filePicker = filePicker;
            _fileOpener = fileOpener;
        }

        public async Task<List<TaskAttachment>> GetAttachmentsAsync(int taskId)
        {
            return await _attachmentRepository.GetAttachmentsAsync(taskId);
        }

        public async Task<Result<TaskAttachment>> AddFile(int taskId)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(taskId);

            string? fileName = _filePicker.PickFile();

            if (fileName is null)
                return Result<TaskAttachment>.Failure(TaskErrors.FileNotSelected);

            FileInfo fileInfo = new FileInfo(fileName);

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
            ArgumentNullException.ThrowIfNull(attachment);

            _fileOpener.Open(attachment.FilePath);
        }
    }
}
