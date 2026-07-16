using Moq;
using WindowsDev.Business.Repositories.Interfaces;
using WindowsDev.Business.Services.TaskService.Comment;
using WindowsDev.Business.Services.UserManager.Interfaces;
using WindowsDev.Domain.TasksModels;

namespace WindowsDev.Tests.Business.TaskService.Comment
{
    public class CommentsServiceTest
    {
        private readonly Mock<ICommentRepository> _commentRepositoryMock;
        private readonly Mock<ICurrentUserService> _currentUserMock;

        public CommentsServiceTest()
        {
            _commentRepositoryMock = new Mock<ICommentRepository>();

            _currentUserMock = new Mock<ICurrentUserService>();
            _currentUserMock
                .SetupGet(x => x.Username)
                .Returns("testuser");
        }

        private CommentsService CreateService()
        {
            return new CommentsService(
                _commentRepositoryMock.Object,
                _currentUserMock.Object);
        }

        [Fact]
        public async Task AddComment_WhenTaskIdLessThan1_ThrowsArgumentOutOfRangeException()
        {
            var service = CreateService();

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => service.AddComment(0, "comment text"));

            _commentRepositoryMock.Verify(
                x => x.AddComments(It.IsAny<Comments>()),
                Times.Never);
        }

        [Fact]
        public async Task AddComment_WhenValid_SavesCommentAndReturnsIt()
        {
            var taskId = 1;
            var commentText = "test comment";

            var service = CreateService();

            var result = await service.AddComment(taskId, commentText);

            Assert.True(result.IsSuccess);

            Assert.Equal(commentText, result.Value.Text);
            Assert.Equal("testuser", result.Value.Author);
            Assert.Equal(taskId, result.Value.TaskId);
            Assert.True((DateTime.UtcNow - result.Value.CreatedAt).TotalSeconds < 1);

            _commentRepositoryMock.Verify(
                x => x.AddComments(It.IsAny<Comments>()),
                Times.Once);
        }

        [Fact]
        public async Task GetComments_WhenCalled_ReturnsCommentsFromRepository()
        {
            var taskId = 1;

            var expectedComments = new List<Comments>
            {
                new()
                {
                    Id = 1,
                    Text = "comment1",
                    TaskId = taskId,
                    CreatedAt = DateTime.UtcNow,
                    Author = "user1"
                },
                new()
                {
                    Id = 2,
                    Text = "comment2",
                    TaskId = taskId,
                    CreatedAt = DateTime.UtcNow,
                    Author = "user2"
                }
            };

            _commentRepositoryMock
                .Setup(x => x.GetComments(taskId))
                .ReturnsAsync(expectedComments);

            var service = CreateService();

            var result = await service.GetComments(taskId);

            Assert.Equal(expectedComments, result);
            Assert.Equal(2, result.Count);

            _commentRepositoryMock.Verify(
                x => x.GetComments(taskId),
                Times.Once);
        }

        [Fact]
        public async Task GetComments_WhenNoComments_ReturnsEmptyList()
        {
            var taskId = 1;

            _commentRepositoryMock
                .Setup(x => x.GetComments(taskId))
                .ReturnsAsync(new List<Comments>());

            var service = CreateService();

            var result = await service.GetComments(taskId);

            Assert.Empty(result);

            _commentRepositoryMock.Verify(
                x => x.GetComments(taskId),
                Times.Once);
        }
    }
}