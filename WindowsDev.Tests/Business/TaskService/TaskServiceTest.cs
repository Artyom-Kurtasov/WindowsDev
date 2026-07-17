using Moq;
using WindowsDev.Business.Repositories.Interfaces;
using WindowsDev.Business.Services.TaskService;
using WindowsDev.Domain.TasksModels;
using WindowsDev.Domain.TasksModels.Enums;
using Service = WindowsDev.Business.Services.TaskService;
using TaskStatus = WindowsDev.Domain.TasksModels.Enums.TaskStatus;

namespace WindowsDev.Tests.Business.TaskService
{
    public class TaskServiceTest
    {
        private readonly Mock<ITaskRepository> _taskRepositoryMock;

        public TaskServiceTest()
        {
            _taskRepositoryMock = new Mock<ITaskRepository>();
        }

        private Service.TaskService CreateService()
        {
            return new Service.TaskService(_taskRepositoryMock.Object);
        }

        [Fact]
        public async Task AddAsync_WhenTaskIsNull_ThrowsArgumentNullException()
        {
            var service = CreateService();

            await Assert.ThrowsAsync<ArgumentNullException>(() => service.AddAsync(null!));

            _taskRepositoryMock.Verify(x => x.AddAsync(It.IsAny<TasksInfo>()), Times.Never);
        }

        [Fact]
        public async Task AddAsync_WhenTaskIsValid_CallsAddAsync()
        {
            var task = CreateTestTask();
            var service = CreateService();

            await service.AddAsync(task);

            _taskRepositoryMock.Verify(x => x.AddAsync(task), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenTaskNotFound_ThrowsArgumentNullException()
        {
            var id = 1;

            _taskRepositoryMock.Setup(x => x.FindTaskById(id)).ReturnsAsync((TasksInfo?)null);

            var service = CreateService();

            await Assert.ThrowsAsync<ArgumentNullException>(() => service.DeleteAsync(id));

            _taskRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<TasksInfo>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_WhenTaskExists_CallsDeleteAsync()
        {
            var task = CreateTestTask();
            task.Id = 1;

            _taskRepositoryMock.Setup(x => x.FindTaskById(task.Id)).ReturnsAsync(task);

            var service = CreateService();

            await service.DeleteAsync(task.Id);

            _taskRepositoryMock.Verify(x => x.DeleteAsync(task), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenTaskIsNull_ThrowsArgumentNullException()
        {
            var service = CreateService();

            await Assert.ThrowsAsync<ArgumentNullException>(() => service.UpdateAsync(null!));

            _taskRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<TasksInfo>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WhenTaskIsValid_CallsUpdateAsync()
        {
            var task = CreateTestTask();
            var service = CreateService();

            await service.UpdateAsync(task);

            _taskRepositoryMock.Verify(x => x.UpdateAsync(task), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public async Task GetTasksAsync_WhenPageLessThan1_SetsPageTo1(int page)
        {
            var filter = new TaskFilter { Page = page, PageSize = 10 };

            var expectedTasks = new List<TasksInfo> { CreateTestTask() };

            _taskRepositoryMock
                .Setup(x => x.GetTasksAsync(filter, 0, 10))
                .ReturnsAsync(expectedTasks);

            var service = CreateService();

            var result = await service.GetTasksAsync(filter);

            Assert.True(result.IsSuccess);
            Assert.Equal(expectedTasks, result.Value);

            _taskRepositoryMock.Verify(x => x.GetTasksAsync(filter, 0, 10), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task GetTasksAsync_WhenPageSizeLessThan1_SetsPageSizeTo1(int pageSize)
        {
            var filter = new TaskFilter { Page = 1, PageSize = pageSize };

            var expectedTasks = new List<TasksInfo> { CreateTestTask() };

            _taskRepositoryMock
                .Setup(x => x.GetTasksAsync(filter, 0, 1))
                .ReturnsAsync(expectedTasks);

            var service = CreateService();

            var result = await service.GetTasksAsync(filter);

            Assert.True(result.IsSuccess);
            Assert.Equal(expectedTasks, result.Value);

            _taskRepositoryMock.Verify(x => x.GetTasksAsync(filter, 0, 1), Times.Once);
        }

        [Fact]
        public async Task GetTasksAsync_WhenValid_CallsRepositoryWithCorrectSkip()
        {
            var filter = new TaskFilter { Page = 3, PageSize = 5 };

            var expectedTasks = new List<TasksInfo> { CreateTestTask() };

            _taskRepositoryMock
                .Setup(x => x.GetTasksAsync(filter, 10, 5))
                .ReturnsAsync(expectedTasks);

            var service = CreateService();

            var result = await service.GetTasksAsync(filter);

            Assert.True(result.IsSuccess);
            Assert.Equal(expectedTasks, result.Value);

            _taskRepositoryMock.Verify(x => x.GetTasksAsync(filter, 10, 5), Times.Once);
        }

        [Fact]
        public async Task GetTasksAsync_WhenFilterIsNull_ThrowsArgumentNullException()
        {
            var service = CreateService();

            await Assert.ThrowsAsync<ArgumentNullException>(() => service.GetTasksAsync(null!));

            _taskRepositoryMock.Verify(
                x => x.GetTasksAsync(It.IsAny<TaskFilter>(), It.IsAny<int>(), It.IsAny<int>()),
                Times.Never
            );
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public async Task GetTasksCountAsync_WhenProjectIdLessThan1_Returns0(int projectId)
        {
            var service = CreateService();

            var result = await service.GetTasksCountAsync(projectId);

            Assert.Equal(0, result);

            _taskRepositoryMock.Verify(x => x.GetTasksCountAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetTasksCountAsync_WhenProjectIdValid_ReturnsCount()
        {
            var projectId = 1;
            var expectedCount = 5;

            _taskRepositoryMock
                .Setup(x => x.GetTasksCountAsync(projectId))
                .ReturnsAsync(expectedCount);

            var service = CreateService();

            var result = await service.GetTasksCountAsync(projectId);

            Assert.Equal(expectedCount, result);

            _taskRepositoryMock.Verify(x => x.GetTasksCountAsync(projectId), Times.Once);
        }

        private TasksInfo CreateTestTask()
        {
            return new TasksInfo
            {
                Name = "Test Task",
                Priority = TaskPriority.Normal,
                Progress = 0,
                Status = TaskStatus.InProgress,
                ProjectId = 1,
                CreatedAt = DateTime.UtcNow,
                DeadLine = DateTime.UtcNow.AddDays(7),
            };
        }
    }
}
