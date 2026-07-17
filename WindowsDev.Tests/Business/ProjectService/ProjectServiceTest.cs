using Moq;
using WindowsDev.Business.Repositories.Interfaces;
using WindowsDev.Business.Services.UserManager;
using WindowsDev.Business.Services.UserManager.Interfaces;
using WindowsDev.Domain.ProjectsModels;
using Service = WindowsDev.Business.Services.ProjectService;

namespace WindowsDev.Tests.Business.ProjectService
{
    public class ProjectServiceTest
    {
        private readonly Mock<IProjectRepository> _projectRepositoryMock;
        private readonly ICurrentUserService _currentUser;

        public ProjectServiceTest()
        {
            _projectRepositoryMock = new Mock<IProjectRepository>();
            _currentUser = new CurrentUserService();
            _currentUser.SetUser(1, "testuser", "Test User");
        }

        private Service.ProjectService CreateService()
        {
            return new Service.ProjectService(_projectRepositoryMock.Object, _currentUser);
        }

        [Fact]
        public async Task AddAsync_WhenProjectIsNull_ThrowsArgumentNullException()
        {
            var service = CreateService();

            await Assert.ThrowsAsync<ArgumentNullException>(() => service.AddAsync(null!));

            _projectRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ProjectsInfo>()), Times.Never);
        }

        [Fact]
        public async Task AddAsync_WhenProjectIsValid_CallsRepositoryAdd()
        {
            var service = CreateService();
            var project = new ProjectsInfo
            {
                Name = "Test Project",
                UserId = _currentUser.UserId,
                CreatedAt = DateTime.UtcNow,
            };

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
            var project = new ProjectsInfo
            {
                Id = 1,
                Name = "Updated Project",
                UserId = _currentUser.UserId,
                CreatedAt = DateTime.UtcNow,
            };

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
            const int page = 1;
            const int size = 10;
            const string searchFilter = "test";
            var expectedProjects = new List<ProjectsInfo>();

            _projectRepositoryMock
                .Setup(x => x.GetProjectsAsync(page, size, _currentUser.UserId, searchFilter))
                .ReturnsAsync(expectedProjects);

            var result = await service.GetProjectsAsync(page, size, searchFilter);

            Assert.Equal(expectedProjects, result);
            _projectRepositoryMock.Verify(
                x => x.GetProjectsAsync(page, size, _currentUser.UserId, searchFilter),
                Times.Once
            );
        }

        [Fact]
        public async Task GetProjectsAsync_WhenPageLessThan1_SetsPageTo1()
        {
            var service = CreateService();
            const int invalidPage = 0;
            const int size = 10;
            const int expectedPage = 1;
            var expectedProjects = new List<ProjectsInfo>();

            _projectRepositoryMock
                .Setup(x =>
                    x.GetProjectsAsync(expectedPage, size, _currentUser.UserId, It.IsAny<string>())
                )
                .ReturnsAsync(expectedProjects);

            var result = await service.GetProjectsAsync(invalidPage, size);

            Assert.Equal(expectedProjects, result);
            _projectRepositoryMock.Verify(
                x =>
                    x.GetProjectsAsync(expectedPage, size, _currentUser.UserId, It.IsAny<string>()),
                Times.Once
            );
        }

        [Fact]
        public async Task GetProjectsAsync_WhenSizeLessThan1_SetsSizeTo1()
        {
            var service = CreateService();
            const int page = 1;
            const int invalidSize = 0;
            const int expectedSize = 1;
            var expectedProjects = new List<ProjectsInfo>();

            _projectRepositoryMock
                .Setup(x =>
                    x.GetProjectsAsync(page, expectedSize, _currentUser.UserId, It.IsAny<string>())
                )
                .ReturnsAsync(expectedProjects);

            var result = await service.GetProjectsAsync(page, invalidSize);

            Assert.Equal(expectedProjects, result);
            _projectRepositoryMock.Verify(
                x =>
                    x.GetProjectsAsync(page, expectedSize, _currentUser.UserId, It.IsAny<string>()),
                Times.Once
            );
        }

        [Fact]
        public async Task GetProjectsCountAsync_WhenCalled_ReturnsCount()
        {
            var service = CreateService();
            const int expectedCount = 5;

            _projectRepositoryMock
                .Setup(x => x.GetProjectsCountAsync(_currentUser.UserId))
                .ReturnsAsync(expectedCount);

            var result = await service.GetProjectsCountAsync();

            Assert.Equal(expectedCount, result);
            _projectRepositoryMock.Verify(
                x => x.GetProjectsCountAsync(_currentUser.UserId),
                Times.Once
            );
        }

        [Fact]
        public async Task GetProjectsCountAsync_WhenNoProjects_ReturnsZero()
        {
            var service = CreateService();

            _projectRepositoryMock
                .Setup(x => x.GetProjectsCountAsync(_currentUser.UserId))
                .ReturnsAsync(0);

            var result = await service.GetProjectsCountAsync();

            Assert.Equal(0, result);
            _projectRepositoryMock.Verify(
                x => x.GetProjectsCountAsync(_currentUser.UserId),
                Times.Once
            );
        }
    }
}
