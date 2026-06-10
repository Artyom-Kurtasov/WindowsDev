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
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public async Task GetProjectsAsync_WhenPageLessThen1_SetsPage1(int page)
        {
            var reader = new Service.ProjectService(_projectRepositoryMock.Object, _currentUser);

            var projects = CreateTestProjects();

            _projectRepositoryMock
                .Setup(x => x.GetProjectsAsync(1, 1, _currentUser.UserId, It.IsAny<string>()))
                .ReturnsAsync(projects);

            var result = await reader.GetProjectsAsync(page, 1);

            _projectRepositoryMock
                .Verify(x => x.GetProjectsAsync(1, 1, _currentUser.UserId, It.IsAny<string>()), Times.Once);

            Assert.Equal(projects.Count, result.Count);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task GetProjectsAsync_WhenSizeLessThen1_SetsSize1(int size)
        {
            var reader = new Service.ProjectService(_projectRepositoryMock.Object, _currentUser);

            var projects = CreateTestProjects();

            _projectRepositoryMock
                .Setup(x => x.GetProjectsAsync(1, 1, _currentUser.UserId, It.IsAny<string>()))
                .ReturnsAsync(projects);

            var result = await reader.GetProjectsAsync(1, size);

            _projectRepositoryMock
                .Verify(x => x.GetProjectsAsync(1, 1, _currentUser.UserId, It.IsAny<string>()), Times.Once);

            Assert.Equal(projects.Count, result.Count);
        }

        [Fact]
        public async Task AddAsync_WhenProjectIsNull_ThrowArgumentNullException()
        {
            var writer = new Service.ProjectService(_projectRepositoryMock.Object, _currentUser);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await writer.AddAsync(null!));

            _projectRepositoryMock
                .Verify(x => x.AddAsync(It.IsAny<ProjectsInfo>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WhenProjectIsNull_ThrowArgumentNullException()
        {
            var writer = new Service.ProjectService(_projectRepositoryMock.Object, _currentUser);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await writer.UpdateAsync(null!));

            _projectRepositoryMock
                .Verify(x => x.UpdateAsync(It.IsAny<ProjectsInfo>()), Times.Never);
        }

        private List<ProjectsInfo> CreateTestProjects()
        {
            var projects = new List<ProjectsInfo>();

            for (int i = 0; i < 7; i++)
            {
                projects.Add(new ProjectsInfo
                {
                    Name = $"Test{i}",
                    UserId = 1,
                    CreatedAt = DateTime.UtcNow.AddDays(-i)
                });
            }

            return projects;
        }
    }
}
