using AutoMapper;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;
using WindowsDev.ApiClient.ProjectsClient;
using WindowsDev.Application.Identity.Cryptography;
using WindowsDev.Contracts.Api.Projects;
using WindowsDev.Contracts.UI.Projects;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;

namespace WindowsDev.Tests.ApiClient;

public class ProjectsApiClientTests
{
    private readonly Mock<ISecureTokenStorage> _tokenStorageMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<HttpMessageHandler> _handlerMock;
    private readonly HttpClient _httpClient;
    private readonly ProjectsApiClient _client;

    public ProjectsApiClientTests()
    {
        _tokenStorageMock = new Mock<ISecureTokenStorage>();
        _mapperMock = new Mock<IMapper>();
        _handlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost/"),
        };
        _client = new ProjectsApiClient(_httpClient, _tokenStorageMock.Object, _mapperMock.Object);
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
    public async Task AddAsync_WhenSuccess_ReturnsSuccessResult()
    {
        var form = new ProjectForm("name", "desc");
        var apiRequest = new AddProjectRequest { Name = "name", Description = "desc" };
        var responseDto = new AddProjectResponse { Id = 1, Name = "name" };
        var info = new ProjectInfo(1, "name", "desc", DateTime.UtcNow);

        _mapperMock.Setup(x => x.Map<AddProjectRequest>(form)).Returns(apiRequest);
        SetupHttpResponse(HttpStatusCode.OK, responseDto);
        _mapperMock.Setup(x => x.Map<ProjectInfo>(It.IsAny<AddProjectResponse>())).Returns(info);

        var result = await _client.AddAsync(form);

        Assert.True(result.IsSuccess);
        Assert.Same(info, result.Value);
    }

    [Fact]
    public async Task DeleteAsync_WhenCalled_ReturnsSuccessResult()
    {
        SetupHttpResponse(HttpStatusCode.NoContent);

        var result = await _client.DeleteAsync(1);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }

    [Fact]
    public async Task UpdateAsync_WhenSuccess_ReturnsSuccessResult()
    {
        var form = new UpdateProjectForm(1, "name", "desc");
        var responseDto = new UpdateProjectResponse { Id = 1, Name = "name", Description = "desc" };
        var info = new UpdateProjectInfo(1, 1, "name", "desc", DateTime.UtcNow);

        SetupHttpResponse(HttpStatusCode.OK, responseDto);
        _mapperMock.Setup(x => x.Map<UpdateProjectInfo>(It.IsAny<UpdateProjectResponse>())).Returns(info);

        var result = await _client.UpdateAsync(1, form);

        Assert.True(result.IsSuccess);
        Assert.Same(info, result.Value);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ReturnsFailureResult()
    {
        var form = new UpdateProjectForm(1, "name", "desc");

        SetupHttpResponse(HttpStatusCode.NotFound);

        var result = await _client.UpdateAsync(1, form);

        Assert.True(result.IsFailure);
        Assert.Equal(ProjectsErrors.ProjectNotFound, result.Error);
    }

    [Fact]
    public async Task GetProjectsAsync_WhenSuccess_ReturnsSuccessResult()
    {
        var responseDto = new List<GetProjectsResponse>
        {
            new() { Id = 1, Name = "name", Description = "desc" }
        };
        var info = new List<ProjectInfo>
        {
            new(1, "name", "desc", DateTime.UtcNow)
        };

        SetupHttpResponse(HttpStatusCode.OK, responseDto);
        _mapperMock.Setup(x => x.Map<List<ProjectInfo>>(It.IsAny<List<GetProjectsResponse>>())).Returns(info);

        var result = await _client.GetProjectsAsync(1, 10, "search");

        Assert.True(result.IsSuccess);
        Assert.Same(info, result.Value);
    }

    [Fact]
    public async Task GetCountAsync_WhenSuccess_ReturnsSuccessResult()
    {
        var responseDto = new GetProjectsCountResponse { TotalCount = 5 };

        SetupHttpResponse(HttpStatusCode.OK, responseDto);

        var result = await _client.GetCountAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value);
    }

    [Fact]
    public async Task GetByIdAsync_WhenSuccess_ReturnsSuccessResult()
    {
        var responseDto = new GetProjectsResponse { Id = 1, Name = "name", Description = "desc" };
        var info = new ProjectInfo(1, "name", "desc", DateTime.UtcNow);

        SetupHttpResponse(HttpStatusCode.OK, responseDto);
        _mapperMock.Setup(x => x.Map<ProjectInfo>(It.IsAny<GetProjectsResponse>())).Returns(info);

        var result = await _client.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Same(info, result.Value);
    }
}