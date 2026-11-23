using System.Net;
using System.Net.Http.Json;
using AutoMapper;
using Moq;
using Moq.Protected;
using WindowsDev.ApiClient.CommentClient;
using WindowsDev.Application.Identity.Cryptography;
using WindowsDev.Contracts.Api.Comments;
using WindowsDev.Contracts.UI.Comments;
using Xunit;

namespace WindowsDev.Tests.ApiClient;

public class CommentApiClientTests
{
    private readonly Mock<ISecureTokenStorage> _tokenStorageMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<HttpMessageHandler> _handlerMock;
    private readonly HttpClient _httpClient;
    private readonly CommentApiClient _client;

    public CommentApiClientTests()
    {
        _tokenStorageMock = new Mock<ISecureTokenStorage>();
        _mapperMock = new Mock<IMapper>();
        _handlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost/"),
        };
        _client = new CommentApiClient(_httpClient, _tokenStorageMock.Object, _mapperMock.Object);
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, object? content = null)
    {
        var response = new HttpResponseMessage(statusCode);
        if (content != null)
        {
            response.Content = JsonContent.Create(content);
        }

        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
    }

    [Fact]
    public async Task GetCommentAsync_WhenSuccess_ReturnsSuccessResult()
    {
        var responseDto = new GetCommentResponse { Id = 1, Text = "text", Author = "author", CreatedAt = DateTime.UtcNow, TaskId = 1 };
        var info = new CommentInfo(1, "text", responseDto.CreatedAt, "author", 1);

        SetupHttpResponse(HttpStatusCode.OK, responseDto);
        _mapperMock.Setup(x => x.Map<CommentInfo>(It.IsAny<GetCommentResponse>())).Returns(info);

        var result = await _client.GetCommentAsync(1, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Same(info, result.Value);
    }

    [Fact]
    public async Task GetCommentAsync_WhenUnauthorized_ReturnsFailureResult()
    {
        SetupHttpResponse(HttpStatusCode.Unauthorized);

        var result = await _client.GetCommentAsync(1, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Unauthorized", result.Error);
    }

    [Fact]
    public async Task AddCommentAsync_WhenSuccess_ReturnsSuccessResult()
    {
        var request = new AddCommentForm(1, "text");
        var responseDto = new AddCommentResponse { Id = 1, Text = "text", Author = "author", CreatedAt = DateTime.UtcNow, TaskId = 1 };
        var info = new CommentInfo(1, "text", responseDto.CreatedAt, "author", 1);

        SetupHttpResponse(HttpStatusCode.OK, responseDto);
        _mapperMock.Setup(x => x.Map<CommentInfo>(It.IsAny<AddCommentResponse>())).Returns(info);

        var result = await _client.AddCommentAsync(request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Same(info, result.Value);
    }

    [Fact]
    public async Task GetCommentsAsync_WhenSuccess_ReturnsSuccessResult()
    {
        var responseDto = new List<GetCommentResponse>
        {
            new() { Id = 1, Text = "text", Author = "author", CreatedAt = DateTime.UtcNow, TaskId = 1 }
        };
        var info = new List<CommentInfo>
        {
            new(1, "text", responseDto[0].CreatedAt, "author", 1)
        };

        SetupHttpResponse(HttpStatusCode.OK, responseDto);
        _mapperMock.Setup(x => x.Map<List<CommentInfo>>(It.IsAny<List<GetCommentResponse>>())).Returns(info);

        var result = await _client.GetCommentsAsync(1, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Same(info, result.Value);
    }
}