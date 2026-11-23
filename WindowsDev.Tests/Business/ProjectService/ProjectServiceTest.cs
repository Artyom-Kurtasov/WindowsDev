using Moq;
using WindowsDev.Application.Projects;
using WindowsDev.Domain.Entities;

namespace WindowsDev.Tests.Business.ProjectService;

public class ProjectServiceTest
{
    private readonly Mock<IProjectRepository> _projectRepositoryMock;

    public ProjectServiceTest()
    {
        _projectRepositoryMock = new Mock<IProjectRepository>();
    }

    private Application.Projects.ProjectService CreateService() =>
        new Application.Projects.ProjectService(_projectRepositoryMock.Object);

    private static ProjectsInfo CreateProject(int id = 0, int userId = 1, string name = "Test Project") =>
        new ProjectsInfo
        {
            Id = id,
            Name = name,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
        };

    [Fact]
    public async Task AddAsync_WhenProjectIsNull_ThrowsArgumentNullException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.AddAsync(null!));

        _projectRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<ProjectsInfo>()),
            Times.Never
        );
    }

    [Fact]
    public async Task AddAsync_WhenProjectIsValid_CallsRepositoryAdd()
    {
        var service = CreateService();
        var project = CreateProject(userId: 42);

        await service.AddAsync(project);

        _projectRepositoryMock.Verify(x => x.AddAsync(project), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenProjectIsNull_ThrowsArgumentNullException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.UpdateAsync(null!));

        _projectRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<ProjectsInfo>()),
            Times.Never
        );
    }

    [Fact]
    public async Task UpdateAsync_WhenProjectIsValid_CallsRepositoryUpdate()
    {
        var service = CreateService();
        var project = CreateProject(id: 1, name: "Updated Project");

        await service.UpdateAsync(project);

        _projectRepositoryMock.Verify(x => x.UpdateAsync(project), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenCalled_CallsRepositoryDelete()
    {
        var service = CreateService();
        const int projectId = 1;

        await service.DeleteAsync(projectId);

        _projectRepositoryMock.Verify(x => x.DeleteAsync(projectId), Times.Once);
    }

    [Fact]
    public async Task GetProjectsAsync_WithValidParams_CallsRepositoryWithCorrectParams()
    {
        var service = CreateService();
        const int userId = 42;
        const int page = 2;
        const int size = 10;
        const string searchFilter = "test";
        var expectedProjects = new List<ProjectsInfo>();

        _projectRepositoryMock
            .Setup(x => x.GetProjectsAsync(page, size, userId, searchFilter))
            .ReturnsAsync(expectedProjects);

        var result = await service.GetProjectsAsync(userId, page, size, searchFilter);

        Assert.Same(expectedProjects, result);
        _projectRepositoryMock.Verify(
            x => x.GetProjectsAsync(page, size, userId, searchFilter),
            Times.Once
        );
    }

    [Fact]
    public async Task GetProjectsAsync_WhenPageLessThan1_SetsPageTo1()
    {
        var service = CreateService();
        const int userId = 42;
        const int invalidPage = 0;
        const int size = 10;
        const int expectedPage = 1;
        var expectedProjects = new List<ProjectsInfo>();

        _projectRepositoryMock
            .Setup(x =>
                x.GetProjectsAsync(expectedPage, size, userId, It.IsAny<string>())
            )
            .ReturnsAsync(expectedProjects);

        var result = await service.GetProjectsAsync(userId, invalidPage, size);

        Assert.Same(expectedProjects, result);
        _projectRepositoryMock.Verify(
            x => x.GetProjectsAsync(expectedPage, size, userId, It.IsAny<string>()),
            Times.Once
        );
    }

    [Fact]
    public async Task GetProjectsAsync_WhenSizeLessThan1_SetsSizeTo1()
    {
        var service = CreateService();
        const int userId = 42;
        const int page = 1;
        const int invalidSize = 0;
        const int expectedSize = 1;
        var expectedProjects = new List<ProjectsInfo>();

        _projectRepositoryMock
            .Setup(x =>
                x.GetProjectsAsync(page, expectedSize, userId, It.IsAny<string>())
            )
            .ReturnsAsync(expectedProjects);

        var result = await service.GetProjectsAsync(userId, page, invalidSize);

        Assert.Same(expectedProjects, result);
        _projectRepositoryMock.Verify(
            x => x.GetProjectsAsync(page, expectedSize, userId, It.IsAny<string>()),
            Times.Once
        );
    }

    [Fact]
    public async Task GetProjectsCountAsync_WhenCalled_ReturnsCount()
    {
        var service = CreateService();
        const int userId = 42;
        const int expectedCount = 5;

        _projectRepositoryMock
            .Setup(x => x.GetProjectsCountAsync(userId))
            .ReturnsAsync(expectedCount);

        var result = await service.GetProjectsCountAsync(userId);

        Assert.Equal(expectedCount, result);
        _projectRepositoryMock.Verify(
            x => x.GetProjectsCountAsync(userId),
            Times.Once
        );
    }

    [Fact]
    public async Task GetProjectsCountAsync_WhenNoProjects_ReturnsZero()
    {
        var service = CreateService();
        const int userId = 42;

        _projectRepositoryMock
            .Setup(x => x.GetProjectsCountAsync(userId))
            .ReturnsAsync(0);

        var result = await service.GetProjectsCountAsync(userId);

        Assert.Equal(0, result);
        _projectRepositoryMock.Verify(
            x => x.GetProjectsCountAsync(userId),
            Times.Once
        );
    }

    [Fact]
    public async Task GetProjectById_WhenExists_ReturnsProject()
    {
        var service = CreateService();
        var project = CreateProject(id: 7);

        _projectRepositoryMock
            .Setup(x => x.GetProjectById(7))
            .ReturnsAsync(project);

        var result = await service.GetProjectById(7);

        Assert.Same(project, result);
        _projectRepositoryMock.Verify(x => x.GetProjectById(7), Times.Once);
    }

    [Fact]
    public async Task GetProjectById_WhenMissing_ThrowsArgumentNullException()
    {
        var service = CreateService();

        _projectRepositoryMock
            .Setup(x => x.GetProjectById(It.IsAny<int>()))
            .ReturnsAsync((ProjectsInfo)null!);

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.GetProjectById(1));
    }
}