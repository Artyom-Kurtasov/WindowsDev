using Microsoft.AspNetCore.StaticFiles;
using WindowsDev.Api.Attachments;
using WindowsDev.Application.Primitives;
using WindowsDev.Application.Tasks.Attachment;
using WindowsDev.Contracts.Api.Attachments;
using WindowsDev.Domain.Entities;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;

namespace WindowsDev.Api.Attachment;

internal class AttachmentService : IAttachmentService
{
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IWebHostEnvironment _env;
    private readonly string _uploadsFolder;

    public AttachmentService(IWebHostEnvironment env, IAttachmentRepository attachmentRepository)
    {
        _env = env;
        _attachmentRepository = attachmentRepository;

        var webRoot = env.WebRootPath;
        if (string.IsNullOrEmpty(env.WebRootPath))
        {
            webRoot = Path.Combine(env.ContentRootPath, "wwwroot");
        }

        _uploadsFolder = Path.Combine(webRoot, "uploads");
        Directory.CreateDirectory(_uploadsFolder);
    }

    public async Task<Result<string>> UploadFileAsync(IFormFile file, int taskId)
    {
        if (file is null || file.Length == 0)
            return Result<string>.Failure(AttachmentErrors.FileNotSelected);

        var safeName = Path.GetFileName(file.FileName);
        if (string.IsNullOrWhiteSpace(safeName))
            return Result<string>.Failure(AttachmentErrors.InvalidFileName);

        var filePath = Path.Combine(_uploadsFolder, safeName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        var attachment = new TaskAttachment
        {
            FileExtension = Path.GetExtension(file.FileName),
            FilePath = filePath,
            FileName = safeName,
            FileSize = file.Length,
            TaskId = taskId,
        };

        await _attachmentRepository.AddFileInfoToDatabase(attachment);
        return Result<string>.Success(safeName);
    }

    public Result<DownloadFileResponse> DownloadFile(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return Result<DownloadFileResponse>.Failure(AttachmentErrors.FileNameEmpty);

        var safeName = Path.GetFileName(fileName);
        var filePath = Path.Combine(_uploadsFolder, safeName);

        if (!File.Exists(filePath))
            return Result<DownloadFileResponse>.Failure(AttachmentErrors.FileNotFound);

        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(safeName, out var contentType))
            contentType = "application/octet-stream";

        var stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            FileOptions.Asynchronous
        );

        var dto = new DownloadFileResponse(stream, contentType, safeName);
        return Result<DownloadFileResponse>.Success(dto);
    }

    public Result<List<FileListItemResponse>> GetFiles(int taskId)
    {
        if (!Directory.Exists(_uploadsFolder))
            return Result<List<FileListItemResponse>>.Failure(AttachmentErrors.UploadsDirectoryNotFound);

        var taskAttachments = _attachmentRepository.GetAttachmentsAsync(taskId);
        var files = new List<FileListItemResponse>();

        for (int i = 0; i < taskAttachments.Result.Count; i++)
        {
            var attachment = taskAttachments.Result[i];
            files.Add(
                new FileListItemResponse(
                    attachment.FileName,
                    attachment.FileSize,
                    File.GetLastWriteTimeUtc(attachment.FilePath)
                )
            );
        }

        return Result<List<FileListItemResponse>>.Success(files);
    }
}
