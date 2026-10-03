using WindowsDev.Application.Primitives;
using WindowsDev.Domain.Entities;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;

namespace WindowsDev.Application.Tasks.Attachment;

internal class AttachmentService : IAttachmentService
{
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IFileOpener _fileOpener;

    public AttachmentService(
        IAttachmentRepository attachmentRepository,
        IFileOpener fileOpener)
    {
        _attachmentRepository = attachmentRepository;
        _fileOpener = fileOpener;
    }

    public async Task<List<TaskAttachment>> GetAttachmentsAsync(int taskId)
    {
        return await _attachmentRepository.GetAttachmentsAsync(taskId);
    }

    public async Task<Result<TaskAttachment>> GetAttachmentAsync(int attachmentId)
    {
        var attachment = await _attachmentRepository.GetAttachmentAsync(attachmentId);

        if (attachment is null)
            return Result<TaskAttachment>.Failure("!!!!!!!!!!!!");

        return Result<TaskAttachment>.Success(attachment);
    }

    public async Task<Result<TaskAttachment>> AddFile(string fileName, int taskId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(taskId);

        FileInfo fileInfo = new FileInfo(fileName);

        if (fileInfo.Exists)
        {
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

        return Result<TaskAttachment>.Failure(TaskErrors.FileNotSelected);
    }

    public async Task OpenFile(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        _fileOpener.Open(filePath);
    }
}