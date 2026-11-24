using AutoMapper;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;
using WindowsDev.ApiClient.AttachmentClient;
using WindowsDev.Application.Identity.Cryptography;
using WindowsDev.Contracts.Api.Attachments;
using WindowsDev.Contracts.UI.Attachments;

namespace WindowsDev.Tests.ApiClient;

public class AttachmentApiClientTests
{
    private readonly Mock<ISecureTokenStorage> _tokenStorageMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<HttpMessageHandler> _handlerMock = new();
    private readonly AttachmentApiClient _client;

    public AttachmentApiClientTests()
    {
        var httpClient = new HttpClient(_handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };
        _client = new AttachmentApiClient(httpClient, _tokenStorageMock.Object, _mapperMock.Object);
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, object? content = null)
    {
        var response = new HttpResponseMessage(statusCode);
        if (content is not null)
            response.Content = JsonContent.Create(content);

        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
    }

    [Fact]
    public async Task UploadFileAsync_WhenSuccess_ReturnsFileName()
    {
        SetupHttpResponse(HttpStatusCode.Created, new UploadFileResponse("test.txt"));
        var request = new UploadFileForm("test.txt", "text/plain", new MemoryStream("content"u8.ToArray()));

        var result = await _client.UploadFileAsync(request, 1);

        Assert.True(result.IsSuccess);
        Assert.Equal("test.txt", result.Value);
    }

    [Fact]
    public async Task GetFilesAsync_WhenSuccess_ReturnsMappedFiles()
    {
        var response = new List<FileListItemResponse>
        {
            new("test.txt", 7, DateTime.UtcNow)
        };
        var files = new List<FileItem>
        {
            new("test.txt", 7, response[0].LastModifiedUtc)
        };
        SetupHttpResponse(HttpStatusCode.OK, response);
        _mapperMock.Setup(x => x.Map<List<FileItem>>(It.IsAny<List<FileListItemResponse>>())).Returns(files);

        var result = await _client.GetFilesAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Same(files, result.Value);
    }

    [Fact]
    public async Task DownloadFileAsync_WhenSuccess_ReturnsContent()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent("content"u8.ToArray())
        };
        response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");
        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var result = await _client.DownloadFileAsync("test file.txt");

        Assert.True(result.IsSuccess);
        Assert.Equal("text/plain", result.Value.ContentType);
        Assert.Equal("test file.txt", result.Value.FileName);
        Assert.Equal("content", await new StreamReader(result.Value.Content).ReadToEndAsync());
    }

    [Fact]
    public async Task DownloadFileAsync_WhenNotFound_ReturnsFailure()
    {
        SetupHttpResponse(HttpStatusCode.NotFound, new
        {
            ErrorMessageCode = "AttachmentNotFound"
        });

        var result = await _client.DownloadFileAsync("missing.txt");

        Assert.True(result.IsFailure);
        Assert.Equal("AttachmentNotFound", result.Error);
    }
}
