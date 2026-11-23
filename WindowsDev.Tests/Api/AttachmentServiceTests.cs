using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Moq;
using WindowsDev.Api.Attachment;

namespace WindowsDev.Tests.Api;

public class AttachmentServiceTests
{
    [Fact]
    public async Task UploadFileAsync_WhenFileIsEmpty_ReturnsFailure()
    {
        var service = CreateService(out var root);

        try
        {
            var result = await service.UploadFileAsync(null!);

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
            var result = await service.UploadFileAsync(file);

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
            var result = service.GetFiles();

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

        return new AttachmentService(environmentMock.Object);
    }
}
