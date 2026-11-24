using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using WindowsDev.Api.Attachment;
using WindowsDev.Application.Tasks.Attachment;
using WindowsDev.Domain.Entities;

namespace WindowsDev.Tests.Api;

public class AttachmentServiceTests
{
    [Fact]
    public async Task UploadFileAsync_WhenFileIsEmpty_ReturnsFailure()
    {
        var service = CreateService(out var root);

        try
        {
            var result = await service.UploadFileAsync(null!, 1);

            Assert.False(result.IsSuccess);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task UploadFileAsync_WhenFileIsValid_SavesFile()
    {
        var service = CreateService(out var root);
        using var stream = new MemoryStream("content"u8.ToArray());
        var file = new FormFile(stream, 0, stream.Length, "file", "folder/test.txt");

        try
        {
            var result = await service.UploadFileAsync(file, 1);

            Assert.True(result.IsSuccess);
            Assert.Equal("test.txt", result.Value);
            Assert.True(File.Exists(Path.Combine(root, "uploads", "test.txt")));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void DownloadFile_WhenFileDoesNotExist_ReturnsFailure()
    {
        var service = CreateService(out var root);

        try
        {
            var result = service.DownloadFile("missing.txt");

            Assert.False(result.IsSuccess);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void DownloadFile_WhenFileExists_ReturnsFile()
    {
        var service = CreateService(out var root);
        var filePath = Path.Combine(root, "uploads", "test.txt");
        File.WriteAllText(filePath, "content");

        try
        {
            var result = service.DownloadFile("test.txt");

            Assert.True(result.IsSuccess);
            Assert.Equal("test.txt", result.Value.FileName);
            Assert.Equal("text/plain", result.Value.ContentType);
            result.Value.Stream.Dispose();
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void GetFiles_WhenFilesExist_ReturnsFiles()
    {
        var service = CreateService(out var root);
        File.WriteAllText(Path.Combine(root, "uploads", "test.txt"), "content");

        try
        {
            var result = service.GetFiles(1);

            Assert.True(result.IsSuccess);
            var file = Assert.Single(result.Value);
            Assert.Equal("test.txt", file.FileName);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    private static AttachmentService CreateService(out string root)
    {
        root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var environmentMock = new Mock<IWebHostEnvironment>();
        environmentMock.SetupGet(x => x.WebRootPath).Returns(root);
        var attachmentRepositoryMock = new Mock<IAttachmentRepository>();
        attachmentRepositoryMock
            .Setup(x => x.GetAttachmentsAsync(It.IsAny<int>()))
            .ReturnsAsync([
                new TaskAttachment
                {
                    FileExtension = ".txt",
                    FileName = "test.txt",
                    FilePath = Path.Combine(root, "uploads", "test.txt"),
                    FileSize = 7,
                    TaskId = 1
                }
            ]);
        attachmentRepositoryMock
            .Setup(x => x.AddFileInfoToDatabase(It.IsAny<TaskAttachment>()))
            .Returns(Task.CompletedTask);

        return new AttachmentService(environmentMock.Object, attachmentRepositoryMock.Object);
    }
}
