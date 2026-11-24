using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WindowsDev.Api.Controllers;
using WindowsDev.Application.Identity;
using WindowsDev.Application.Projects;
using WindowsDev.Contracts.Api.Projects;
using WindowsDev.Domain.Entities;

namespace WindowsDev.Tests.Api;

public class ProjectsControllerTests
{
    private readonly Mock<IProjectService> _projectServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IUserSession> _userSessionMock;
    private readonly Mock<ILogger<ProjectsController>> _loggerMock;

    public ProjectsControllerTests()
    {
        _projectServiceMock = new Mock<IProjectService>();
        _mapperMock = new Mock<IMapper>();
        _userSessionMock = new Mock<IUserSession>();
        _loggerMock = new Mock<ILogger<ProjectsController>>();

        _userSessionMock.SetupGet(x => x.UserId).Returns(42);
        _userSessionMock.SetupGet(x => x.Login).Returns("test-login");
    }

    private ProjectsController CreateController()
    {
        var controller = new ProjectsController(
            _projectServiceMock.Object,
            _loggerMock.Object,
            _mapperMock.Object,
            _userSessionMock.Object
        );

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext(),
        };

        return controller;
    }

    private static ProjectsInfo CreateProject(int id = 1, int userId = 42) =>
        new ProjectsInfo
        {
            Id = id,
            Name = "Test Project",
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
        };

    [Fact]
    public async Task AddAsync_WhenCalled_ReturnsCreatedWithResponse()
    {
        var request = new AddProjectRequest { Name = "New Project" };
        var entity = CreateProject(id: 10);
        var response = new AddProjectResponse { Id = 10, Name = "New Project" };

        _mapperMock
            .Setup(x => x.Map<ProjectsInfo>(request))
            .Returns(entity);

        _mapperMock
            .Setup(x => x.Map<AddProjectResponse>(entity))
            .Returns(response);

        var controller = CreateController();

        var result = await controller.AddAsync(request);

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);
        Assert.Same(response, objectResult.Value);

        _projectServiceMock.Verify(x => x.AddAsync(entity), Times.Once);
        Assert.Equal(42, entity.UserId);
    }

    [Fact]
    public async Task DeleteAsync_WhenCalled_ReturnsNoContent()
    {
        var controller = CreateController();

        var result = await controller.DeleteAsync(5);

        Assert.IsType<NoContentResult>(result);
        _projectServiceMock.Verify(x => x.DeleteAsync(5), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenCalled_ReturnsOkWithResponse()
    {
        var request = new UpdateProjectRequest { Name = "Updated Name" };
        var project = CreateProject(id: 7);
        var response = new UpdateProjectResponse { Id = 7, Name = "Updated Name" };

        _projectServiceMock
            .Setup(x => x.GetProjectById(7))
            .ReturnsAsync(project);

        _mapperMock
            .Setup(x => x.Map(request, project))
            .Returns(project);

        _mapperMock
            .Setup(x => x.Map<UpdateProjectResponse>(project))
            .Returns(response);

        var controller = CreateController();

        var result = await controller.UpdateAsync(7, request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);

        _projectServiceMock.Verify(x => x.UpdateAsync(project), Times.Once);
    }

    [Fact]
    public async Task GetProjectsAsync_WhenCalled_ReturnsOkWithMappedList()
    {
        var projects = new List<ProjectsInfo>
        {
            CreateProject(id: 1),
            CreateProject(id: 2),
        };

        var response = new List<GetProjectsResponse>
        {
            new() { Id = 1, Name = "Test Project" },
            new() { Id = 2, Name = "Test Project" },
        };

        _projectServiceMock
            .Setup(x => x.GetProjectsAsync(42, 1, 10, "search"))
            .ReturnsAsync(projects);

        _mapperMock
            .Setup(x => x.Map<List<GetProjectsResponse>>(projects))
            .Returns(response);

        var controller = CreateController();

        var result = await controller.GetProjectsAsync(1, 10, "search");

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
    }

    [Fact]
    public async Task GetProjectsAsync_WhenSearchFilterIsNull_PassesEmptyStringToService()
    {
        var projects = new List<ProjectsInfo>();
        var response = new List<GetProjectsResponse>();

        _projectServiceMock
            .Setup(x => x.GetProjectsAsync(42, 1, 10, ""))
            .ReturnsAsync(projects);

        _mapperMock
            .Setup(x => x.Map<List<GetProjectsResponse>>(projects))
            .Returns(response);

        var controller = CreateController();

        var result = await controller.GetProjectsAsync(1, 10, null);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);

        _projectServiceMock.Verify(x => x.GetProjectsAsync(42, 1, 10, ""), Times.Once);
    }

    [Fact]
    public async Task GetCountAsync_WhenCalled_ReturnsOkWithCount()
    {
        _projectServiceMock
            .Setup(x => x.GetProjectsCountAsync(42))
            .ReturnsAsync(15);

        var controller = CreateController();

        var result = await controller.GetCountAsync();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<GetProjectsCountResponse>(ok.Value);
        Assert.Equal(15, response.TotalCount);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCalled_ReturnsOkWithResponse()
    {
        var project = CreateProject(id: 5);
        var response = new GetProjectByIdResponse { Id = 5, Name = "Test Project" };

        _projectServiceMock
            .Setup(x => x.GetProjectById(5))
            .ReturnsAsync(project);

        _mapperMock
            .Setup(x => x.Map<GetProjectByIdResponse>(project))
            .Returns(response);

        var controller = CreateController();

        var result = await controller.GetByIdAsync(5);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
    }
}