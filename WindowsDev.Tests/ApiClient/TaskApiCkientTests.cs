using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;
using WindowsDev.ApiClient.TaskClient;
using WindowsDev.Application.Identity.Cryptography;
using WindowsDev.Contracts.Api.Common;
using WindowsDev.Contracts.Api.Tasks;
using WindowsDev.Contracts.UI.Tasks;
using WindowsDev.Domain.Enums;
using TaskStatus = WindowsDev.Domain.Enums.TaskStatus;

namespace WindowsDev.Tests.ApiClient;

public class TaskApiClientTests
{
    private readonly Mock<ISecureTokenStorage> _tokenStorageMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<TaskApiClient>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _handlerMock;
    private readonly HttpClient _httpClient;
    private readonly TaskApiClient _client;

    public TaskApiClientTests()
    {
        _tokenStorageMock = new Mock<ISecureTokenStorage>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<TaskApiClient>>();
        _handlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost/"),
        };
        _client = new TaskApiClient(_httpClient, _tokenStorageMock.Object, _mapperMock.Object, _loggerMock.Object);
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
        var form = new TaskForm(
            "name",
            "desc",
            TaskPriority.Low,
            TaskStatus.InProgress,
            DateTime.UtcNow,
            0,
            1);

        var apiRequest = new AddTaskRequest
        {
            Name = "name",
            Description = "desc",
            Priority = TaskPriority.Low,
            Status = TaskStatus.InProgress,
            Deadline = form.Deadline,
            Progress = 0,
            ProjectId = 1
        };

        var responseDto = new AddTaskResponse
        {
            Name = "name",
            Description = "desc",
            Priority = TaskPriority.Low,
            Status = TaskStatus.InProgress,
            Deadline = form.Deadline,
            CreatedAt = DateTime.UtcNow,
            Progress = 0,
            ProjectId = 1,
        };

        var info = new Mock<TaskInfo>().Object;

        _mapperMock.Setup(x => x.Map<AddTaskRequest>(form)).Returns(apiRequest);
        SetupHttpResponse(HttpStatusCode.OK, responseDto);
        _mapperMock.Setup(x => x.Map<TaskInfo>(It.IsAny<AddTaskResponse>())).Returns(info);

        var result = await _client.AddAsync(form);

        Assert.True(result.IsSuccess);
        Assert.Same(info, result.Value);
    }

    [Fact]
    public async Task DeleteAsync_WhenCalled_ReturnsSuccessResult()
    {
        SetupHttpResponse(HttpStatusCode.NoContent);

        var result = await _client.DeleteAsync(1, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }

    [Fact]
    public async Task UpdateAsync_WhenSuccess_ReturnsSuccessResult()
    {
        var form = new UpdateTaskForm(
            50,
            "name",
            "desc",
            TaskPriority.High,
            TaskStatus.InProgress,
            DateTime.UtcNow);

        var apiRequest = new UpdateTaskRequest
        {
            Name = "name",
            Description = "desc",
            Priority = TaskPriority.High,
            Status = TaskStatus.InProgress,
            Deadline = form.Deadline,
            Progress = 50
        };

        var responseDto = new UpdateTaskResponse
        {
            Id = 1,
            Name = "name",
            Description = "desc",
            Priority = TaskPriority.High,
            Status = TaskStatus.InProgress,
            Deadline = form.Deadline,
            CreatedAt = DateTime.UtcNow,
            Progress = 50,
            ProjectId = 1,
        };

        var info = new UpdateTaskInfo(
            1,
            "name",
            "desc",
            TaskPriority.High,
            TaskStatus.InProgress,
            form.Deadline,
            responseDto.CreatedAt,
            50,
            1);

        _mapperMock.Setup(x => x.Map<UpdateTaskRequest>(form)).Returns(apiRequest);
        SetupHttpResponse(HttpStatusCode.OK, responseDto);
        _mapperMock.Setup(x => x.Map<UpdateTaskInfo>(It.IsAny<UpdateTaskResponse>())).Returns(info);

        var result = await _client.UpdateAsync(1, form);

        Assert.True(result.IsSuccess);
        Assert.Same(info, result.Value);
    }

    [Fact]
    public async Task UpdateAsync_WhenFailure_ReturnsFailureResult()
    {
        var form = new UpdateTaskForm(
            50,
            "name",
            "desc",
            TaskPriority.High,
            TaskStatus.InProgress,
            DateTime.UtcNow);

        var apiRequest = new UpdateTaskRequest();
        var problem = new ProblemDetails { Title = "Error" };

        _mapperMock.Setup(x => x.Map<UpdateTaskRequest>(form)).Returns(apiRequest);
        SetupHttpResponse(HttpStatusCode.BadRequest, problem);

        var result = await _client.UpdateAsync(1, form);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task GetTasksAsync_WhenSuccess_ReturnsSuccessResult()
    {
        var responseDto = new List<GetTasksResponse>
        {
            new()
            {
                Id = 1,
                Name = "name",
                Description = "desc",
                Priority = TaskPriority.Low,
                Progress = 0,
                Status = TaskStatus.InProgress,
                ProjectId = 1,
                CreatedAt = DateTime.UtcNow,
                Deadline = DateTime.UtcNow,
            }
        };

        var info = new List<TaskInfo> { new Mock<TaskInfo>().Object };

        SetupHttpResponse(HttpStatusCode.OK, responseDto);
        _mapperMock.Setup(x => x.Map<List<TaskInfo>>(It.IsAny<List<GetTasksResponse>>())).Returns(info);

        var result = await _client.GetTasksAsync(1, "search", 1, 10, new List<TaskStatus>());

        Assert.True(result.IsSuccess);
        Assert.Same(info, result.Value);
    }

    [Fact]
    public async Task GetTasksCountAsync_WhenSuccess_ReturnsSuccessResult()
    {
        var responseDto = new GetTasksCountResponse { TotalCount = 5 };

        SetupHttpResponse(HttpStatusCode.OK, responseDto);

        var result = await _client.GetTasksCountAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value);
    }

    [Fact]
    public async Task GetTaskAsync_WhenSuccess_ReturnsSuccessResult()
    {
        var responseDto = new GetTaskByIdResponse
        {
            Name = "name",
            Description = "desc",
            Priority = TaskPriority.Low,
            Status = TaskStatus.InProgress,
            Deadline = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            Progress = 0,
            ProjectId = 1,
        };

        var info = new Mock<TaskInfo>().Object;

        SetupHttpResponse(HttpStatusCode.OK, responseDto);
        _mapperMock.Setup(x => x.Map<TaskInfo>(It.IsAny<GetTaskByIdResponse>())).Returns(info);

        var result = await _client.GetTaskAsync(1, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Same(info, result.Value);
    }

    [Fact]
    public async Task GetTaskAsync_WhenFailure_ReturnsFailureResult()
    {
        var problem = new ProblemDetails { Title = "Error" };

        SetupHttpResponse(HttpStatusCode.BadRequest, problem);

        var result = await _client.GetTaskAsync(1, CancellationToken.None);

        Assert.True(result.IsFailure);
    }
}