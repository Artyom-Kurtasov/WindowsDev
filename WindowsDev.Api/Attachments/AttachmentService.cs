using Microsoft.AspNetCore;
using Microsoft.AspNetCore.StaticFiles;
using WindowsDev.Api.Attachments;
using WindowsDev.Application.Primitives;
using WindowsDev.Contracts.Api.Attachments;

namespace WindowsDev.Api.Attachment;

internal class AttachmentService : IAttachmentService
{
    private readonly IWebHostEnvironment _env;
    private readonly string _uploadsFolder;

    public AttachmentService(IWebHostEnvironment env)
    {
        _env = env;

        var webRoot = env.WebRootPath;
        if (string.IsNullOrEmpty(env.WebRootPath))
        {
            webRoot = Path.Combine(env.ContentRootPath, "wwwroot");
        }

        _uploadsFolder = Path.Combine(webRoot, "uploads");
        Directory.CreateDirectory(_uploadsFolder); 
    }

    public async Task<Result<string>> UploadFileAsync(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return Result<string>.Failure("File not selected");

        var safeName = Path.GetFileName(file.FileName);
        if (string.IsNullOrWhiteSpace(safeName))
            return Result<string>.Failure("Invalid file name.");

        var filePath = Path.Combine(_uploadsFolder, safeName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return Result<string>.Success(safeName);
    }

    public Result<DownloadFileResponse> DownloadFile(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return Result<DownloadFileResponse>.Failure("File name cannot be empty.");

        var safeName = Path.GetFileName(fileName);
        var filePath = Path.Combine(_uploadsFolder, safeName);

        if (!File.Exists(filePath))
            return Result<DownloadFileResponse>.Failure(
                $"Failed to download file {safeName}. File does not exist."
            );

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

    public Result<List<FileListItemResponse>> GetFiles()
    {
        if (!Directory.Exists(_uploadsFolder))
            return Result<List<FileListItemResponse>>.Failure(
                $"Uploads directory '{_uploadsFolder}' not found."
            );

        var files = new DirectoryInfo(_uploadsFolder)
            .GetFiles()
            .OrderByDescending(f => f.LastWriteTimeUtc)
            .Select(f => new FileListItemResponse(f.Name, f.Length, f.LastWriteTimeUtc))
            .ToList();

        return Result<List<FileListItemResponse>>.Success(files);
    }
}
