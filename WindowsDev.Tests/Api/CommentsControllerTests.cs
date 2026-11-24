using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WindowsDev.Api.Controllers;
using WindowsDev.Api.ProblemDetails;
using WindowsDev.Application.Primitives;
using WindowsDev.Application.Tasks.Comment;
using WindowsDev.Contracts.Api.Comments;
using WindowsDev.Domain.Entities;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;

namespace WindowsDev.Tests.Api;

public class CommentsControllerTests
{
    private readonly Mock<ICommentService> _commentServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<CommentsController>> _loggerMock;

    public CommentsControllerTests()
    {
        _commentServiceMock = new Mock<ICommentService>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<CommentsController>>();
    }

    private CommentsController CreateController()
    {
        var controller = new CommentsController(
            _commentServiceMock.Object,
            _mapperMock.Object,
            _loggerMock.Object
        );

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext(),
        };

        return controller;
    }

    private static TaskComment CreateComment(int id = 1, int taskId = 10, string text = "text") =>
        new TaskComment
        {
            Id = id,
            TaskId = taskId,
            Text = text,
            CreatedAt = DateTime.UtcNow,
            Author = "author",
        };

    private static GetCommentResponse CreateGetResponse(int id, string text) =>
        new GetCommentResponse
        {
            Id = id,
            Text = text,
            CreatedAt = DateTime.UtcNow,
            Author = "author",
            TaskId = 10,
        };

    private static AddCommentResponse CreateAddResponse(int id, string text) =>
        new AddCommentResponse
        {
            Id = id,
            Text = text,
            CreatedAt = DateTime.UtcNow,
            Author = "author",
            TaskId = 10,
        };

    [Fact]
    public async Task GetCommentAsync_WhenSuccess_ReturnsOkWithMappedResponse()
    {
        var comment = CreateComment(id: 5);
        var response = CreateGetResponse(5, "hello");

        _commentServiceMock
            .Setup(x => x.GetCommentAsync(5))
            .ReturnsAsync(Result<TaskComment>.Success(comment));

        _mapperMock
            .Setup(x => x.Map<GetCommentResponse>(comment))
            .Returns(response);

        var controller = CreateController();

        var result = await controller.GetCommentAsync(5);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
    }

    [Fact]
    public async Task GetCommentAsync_WhenNotFound_ReturnsNotFoundProblem()
    {
        _commentServiceMock
            .Setup(x => x.GetCommentAsync(42))
            .ReturnsAsync(Result<TaskComment>.Failure(CommentErrors.CommentNotFound));

        var controller = CreateController();

        var result = await controller.GetCommentAsync(42);

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetCommentAsync_WhenNotFound_ReturnsNeutralProblemTitle()
    {
        _commentServiceMock
            .Setup(x => x.GetCommentAsync(42))
            .ReturnsAsync(Result<TaskComment>.Failure(CommentErrors.CommentNotFound));

        var controller = CreateController();

        var result = await controller.GetCommentAsync(42);

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(CommentProblemDetails.CommentNotFound, problem.Title);
    }

    [Fact]
    public async Task GetCommentAsync_WhenNotFound_DoesNotMap()
    {
        _commentServiceMock
            .Setup(x => x.GetCommentAsync(42))
            .ReturnsAsync(Result<TaskComment>.Failure(CommentErrors.CommentNotFound));

        var controller = CreateController();

        await controller.GetCommentAsync(42);

        _mapperMock.Verify(
            x => x.Map<GetCommentResponse>(It.IsAny<TaskComment>()),
            Times.Never);
    }

    [Fact]
    public async Task GetCommentsAsync_WhenCalled_ReturnsOkWithMappedList()
    {
        var comments = new List<TaskComment>
        {
            CreateComment(id: 1),
            CreateComment(id: 2),
        };

        var response = new List<GetCommentResponse>
        {
            CreateGetResponse(1, "text1"),
            CreateGetResponse(2, "text2"),
        };

        _commentServiceMock
            .Setup(x => x.GetCommentsAsync(10))
            .ReturnsAsync(comments);

        _mapperMock
            .Setup(x => x.Map<List<GetCommentResponse>>(comments))
            .Returns(response);

        var controller = CreateController();

        var result = await controller.GetCommentsAsync(10);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
    }

    [Fact]
    public async Task GetCommentsAsync_WhenEmpty_ReturnsOkWithEmptyList()
    {
        var comments = new List<TaskComment>();
        var response = new List<GetCommentResponse>();

        _commentServiceMock
            .Setup(x => x.GetCommentsAsync(10))
            .ReturnsAsync(comments);

        _mapperMock
            .Setup(x => x.Map<List<GetCommentResponse>>(comments))
            .Returns(response);

        var controller = CreateController();

        var result = await controller.GetCommentsAsync(10);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var value = Assert.IsType<List<GetCommentResponse>>(ok.Value);
        Assert.Empty(value);
    }

    [Fact]
    public async Task AddCommentAsync_WhenCalled_ReturnsCreatedAtActionWithResponse()
    {
        var request = new AddCommentRequest { TaskId = 10, Text = "hello" };
        var comment = CreateComment(id: 7, taskId: 10, text: "hello");
        var response = CreateAddResponse(7, "hello");

        _commentServiceMock
            .Setup(x => x.AddCommentAsync(request.TaskId, request.Text))
            .ReturnsAsync(comment);

        _mapperMock
            .Setup(x => x.Map<AddCommentResponse>(comment))
            .Returns(response);

        var controller = CreateController();

        var result = await controller.AddCommentAsync(request);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Same(response, created.Value);
        Assert.Equal("GetComment", created.ActionName);

        var routeValues = Assert.IsType<Microsoft.AspNetCore.Routing.RouteValueDictionary>(created.RouteValues);
        Assert.Equal(7, routeValues["id"]);
    }

    [Fact]
    public async Task AddCommentAsync_WhenCalled_CallsServiceWithRequestData()
    {
        var request = new AddCommentRequest { TaskId = 10, Text = "hello" };
        var comment = CreateComment(id: 7, taskId: 10, text: "hello");

        _commentServiceMock
            .Setup(x => x.AddCommentAsync(request.TaskId, request.Text))
            .ReturnsAsync(comment);

        _mapperMock
            .Setup(x => x.Map<AddCommentResponse>(comment))
            .Returns(CreateAddResponse(7, "hello"));

        var controller = CreateController();

        await controller.AddCommentAsync(request);

        _commentServiceMock.Verify(
            x => x.AddCommentAsync(request.TaskId, request.Text),
            Times.Once);
    }
}