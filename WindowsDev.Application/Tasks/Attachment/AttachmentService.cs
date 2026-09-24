using WindowsDev.Application.Primitives;
using WindowsDev.Domain.Entities;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;

namespace WindowsDev.Application.Tasks.Attachment;

internal class AttachmentService : IAttachmentService
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

    public async Task<Result<List<TaskAttachment>>> GetAttachmentsAsync(int taskId)
    {
        var attachments = await _attachmentRepository.GetAttachmentsAsync(taskId);
        return Result<List<TaskAttachment>>.Success(attachments);
    }

    public async Task<Result<TaskAttachment>> GetAttachmentAsync(int attachmentId)
    {
        var attachment = await _attachmentRepository.GetAttachmentAsync(attachmentId);

        if (attachment is null)
            return Result<TaskAttachment>.Failure("!!!!!!!!!!!!");

        return Result<TaskAttachment>.Success(attachment);
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

    public async Task OpenFile(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        _fileOpener.Open(filePath);
    }
}