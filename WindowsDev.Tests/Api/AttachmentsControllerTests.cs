using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WindowsDev.Api.Attachments;
using WindowsDev.Api.Controllers;
using WindowsDev.Application.Primitives;
using WindowsDev.Contracts.Api.Attachments;

namespace WindowsDev.Tests.Api;

public class AttachmentsControllerTests
{
    private readonly Mock<IAttachmentService> _attachmentServiceMock = new();
    private readonly Mock<ILogger<AttachmentsController>> _loggerMock = new();

    private AttachmentsController CreateController()
    {
        var controller = new AttachmentsController(
            _attachmentServiceMock.Object,
            _loggerMock.Object
        );

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        return controller;
    }

    [Fact]
    public async Task UploadFileAsync_WhenSuccessful_ReturnsCreated()
    {
        var file = new FormFile(new MemoryStream("content"u8.ToArray()), 0, 7, "file", "test.txt");
        _attachmentServiceMock
            .Setup(x => x.UploadFileAsync(file, 1))
            .ReturnsAsync(Result<string>.Success("test.txt"));

        var result = await CreateController().UploadFileAsync(file, 1);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        var response = Assert.IsType<UploadFileResponse>(created.Value);
        Assert.Equal("test.txt", response.FileName);
    }

    [Fact]
    public async Task UploadFileAsync_WhenFailed_ReturnsBadRequest()
    {
        _attachmentServiceMock
            .Setup(x => x.UploadFileAsync(null!, 1))
            .ReturnsAsync(Result<string>.Failure("error"));

        var result = await CreateController().UploadFileAsync(null!, 1);

        var response = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
    }

    [Fact]
    public void GetFiles_WhenSuccessful_ReturnsOk()
    {
        _attachmentServiceMock
            .Setup(x => x.GetFiles(1))
            .Returns(Result<List<FileListItemResponse>>.Success(
                [new FileListItemResponse("test.txt", 7, DateTime.UtcNow)]));

        var result = CreateController().GetFiles(1);

        var response = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Single(Assert.IsType<List<FileListItemResponse>>(response.Value));
    }

    [Fact]
    public void DownloadFile_WhenSuccessful_ReturnsFile()
    {
        var stream = new MemoryStream("content"u8.ToArray());
        _attachmentServiceMock
            .Setup(x => x.DownloadFile("test.txt"))
            .Returns(Result<DownloadFileResponse>.Success(
                new DownloadFileResponse(stream, "text/plain", "test.txt")));

        var result = CreateController().DownloadFile("test.txt");

        var response = Assert.IsType<FileStreamResult>(result);
        Assert.Equal("text/plain", response.ContentType);
        Assert.Equal("test.txt", response.FileDownloadName);
    }

    [Fact]
    public void DownloadFile_WhenFailed_ReturnsNotFound()
    {
        _attachmentServiceMock
            .Setup(x => x.DownloadFile("missing.txt"))
            .Returns(Result<DownloadFileResponse>.Failure("error"));

        var result = CreateController().DownloadFile("missing.txt");

        var response = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, response.StatusCode);
    }
}
