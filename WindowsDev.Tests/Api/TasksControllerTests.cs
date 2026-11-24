using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WindowsDev.Api.Controllers;
using WindowsDev.Api.ProblemDetails;
using WindowsDev.Application.Primitives;
using WindowsDev.Application.Tasks;
using WindowsDev.Contracts.Api.Tasks;
using WindowsDev.Domain.Entities;
using WindowsDev.Domain.Enums;
using TaskStatus = WindowsDev.Domain.Enums.TaskStatus;

namespace WindowsDev.Tests.Api;

public class TasksControllerTests
{
    private readonly Mock<ITaskService> _taskServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<TasksController>> _loggerMock;

    public TasksControllerTests()
    {
        _taskServiceMock = new Mock<ITaskService>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<TasksController>>();
    }

    private TasksController CreateController()
    {
        var controller = new TasksController(
            _taskServiceMock.Object,
            _mapperMock.Object,
            _loggerMock.Object
        );

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext(),
        };

        return controller;
    }

    private static TasksInfo CreateTask(int id = 1, int projectId = 10) =>
        new TasksInfo
        {
            Id = id,
            ProjectId = projectId,
            Name = "Test Task",
            Description = "Description",
            Status = TaskStatus.InProgress,
            Priority = TaskPriority.Low,
            Deadline = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow,
            Progress = 0,
        };

    [Fact]
    public async Task AddAsync_WhenCalled_ReturnsCreatedAtActionWithResponse()
    {
        var request = new AddTaskRequest
        {
            ProjectId = 10,
            Name = "New Task",
            Description = "Description",
        };

        var entity = CreateTask(id: 7, projectId: 10);
        var response = new AddTaskResponse
        {
            Name = "New Task",
            Description = "Description",
            Priority = TaskPriority.Low,
            Status = TaskStatus.InProgress,
            Deadline = entity.Deadline,
            CreatedAt = entity.CreatedAt,
            Progress = 0,
            ProjectId = 10,
        };

        _mapperMock
            .Setup(x => x.Map<TasksInfo>(request))
            .Returns(entity);

        _mapperMock
            .Setup(x => x.Map<AddTaskResponse>(entity))
            .Returns(response);

        var controller = CreateController();

        var result = await controller.AddAsync(request);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Same(response, created.Value);
        Assert.Equal(nameof(TasksController.GetTaskByIdAsync), created.ActionName);

        var routeValues = Assert.IsType<Microsoft.AspNetCore.Routing.RouteValueDictionary>(created.RouteValues);
        Assert.Equal(7, routeValues["id"]);

        _taskServiceMock.Verify(x => x.AddAsync(entity), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenCalled_ReturnsNoContent()
    {
        var controller = CreateController();

        var result = await controller.DeleteAsync(5);

        Assert.IsType<NoContentResult>(result);
        _taskServiceMock.Verify(x => x.DeleteAsync(5), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenTaskNotFound_ReturnsNotFoundProblem()
    {
        var request = new UpdateTaskRequest { Name = "Updated" };

        _taskServiceMock
            .Setup(x => x.GetAsync(42))
            .ReturnsAsync((TasksInfo?)null);

        var controller = CreateController();

        var result = await controller.UpdateAsync(42, request);

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(TaskProblemDetails.TaskNotFound, problem.Title);
    }

    [Fact]
    public async Task UpdateAsync_WhenCalled_ReturnsOkWithResponse()
    {
        var request = new UpdateTaskRequest
        {
            Name = "Updated",
            Description = "Updated Description",
            Priority = TaskPriority.High,
            Status = TaskStatus.InProgress,
            Deadline = DateTime.UtcNow.AddDays(2),
            Progress = 50,
        };

        var task = CreateTask(id: 5);
        var response = new UpdateTaskResponse
        {
            Id = 5,
            Name = "Updated",
            Description = "Updated Description",
            Priority = TaskPriority.High,
            Status = TaskStatus.InProgress,
            Deadline = request.Deadline,
            CreatedAt = task.CreatedAt,
            Progress = 50,
            ProjectId = task.ProjectId,
        };

        _taskServiceMock
            .Setup(x => x.GetAsync(5))
            .ReturnsAsync(task);

        _mapperMock
            .Setup(x => x.Map(request, task))
            .Returns(task);

        _mapperMock
            .Setup(x => x.Map<UpdateTaskResponse>(task))
            .Returns(response);

        var controller = CreateController();

        var result = await controller.UpdateAsync(5, request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);

        _taskServiceMock.Verify(x => x.UpdateAsync(task), Times.Once);
    }

    [Fact]
    public async Task GetTasksAsync_WhenCalled_ReturnsOkWithMappedList()
    {
        var tasks = new List<TasksInfo>
        {
            CreateTask(id: 1),
            CreateTask(id: 2),
        };

        var response = new List<GetTasksResponse>
        {
            new()
            {
                Id = 1,
                Name = "Test Task",
                Description = "Description",
                Priority = TaskPriority.Low,
                Progress = 0,
                Status = TaskStatus.InProgress,
                ProjectId = 10,
                CreatedAt = tasks[0].CreatedAt,
                Deadline = tasks[0].Deadline,
            },
            new()
            {
                Id = 2,
                Name = "Test Task",
                Description = "Description",
                Priority = TaskPriority.Low,
                Progress = 0,
                Status = TaskStatus.InProgress,
                ProjectId = 10,
                CreatedAt = tasks[1].CreatedAt,
                Deadline = tasks[1].Deadline,
            },
        };

        _taskServiceMock
            .Setup(x => x.GetTasksAsync(It.IsAny<TaskFilter>()))
            .ReturnsAsync(Result<List<TasksInfo>>.Success(tasks));

        _mapperMock
            .Setup(x => x.Map<GetTasksResponse>(tasks[0]))
            .Returns(response[0]);

        _mapperMock
            .Setup(x => x.Map<GetTasksResponse>(tasks[1]))
            .Returns(response[1]);

        var controller = CreateController();

        var result = await controller.GetTasksAsync(
            10,
            "search",
            1,
            10,
            new List<TaskStatus> { TaskStatus.InProgress });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var value = Assert.IsType<List<GetTasksResponse>>(ok.Value);
        Assert.Equal(2, value.Count);
        Assert.Same(response[0], value[0]);
        Assert.Same(response[1], value[1]);
    }

    [Fact]
    public async Task GetTasksCountAsync_WhenCalled_ReturnsOkWithCount()
    {
        _taskServiceMock
            .Setup(x => x.GetTasksCountAsync(10))
            .ReturnsAsync(5);

        var controller = CreateController();

        var result = await controller.GetTasksCountAsync(10);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<GetTasksCountResponse>(ok.Value);
        Assert.Equal(5, response.TotalCount);
    }

    [Fact]
    public async Task GetTaskByIdAsync_WhenTaskNotFound_ReturnsNotFoundProblem()
    {
        _taskServiceMock
            .Setup(x => x.GetAsync(42))
            .ReturnsAsync((TasksInfo?)null);

        var controller = CreateController();

        var result = await controller.GetTaskByIdAsync(42);

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(TaskProblemDetails.TaskNotFound, problem.Title);
    }

    [Fact]
    public async Task GetTaskByIdAsync_WhenCalled_ReturnsOkWithResponse()
    {
        var task = CreateTask(id: 5);
        var response = new GetTaskByIdResponse
        {
            Name = "Test Task",
            Description = "Description",
            Priority = TaskPriority.Low,
            Status = TaskStatus.InProgress,
            Deadline = task.Deadline,
            CreatedAt = task.CreatedAt,
            Progress = 0,
            ProjectId = task.ProjectId,
        };

        _taskServiceMock
            .Setup(x => x.GetAsync(5))
            .ReturnsAsync(task);

        _mapperMock
            .Setup(x => x.Map<GetTaskByIdResponse>(task))
            .Returns(response);

        var controller = CreateController();

        var result = await controller.GetTaskByIdAsync(5);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
    }
}