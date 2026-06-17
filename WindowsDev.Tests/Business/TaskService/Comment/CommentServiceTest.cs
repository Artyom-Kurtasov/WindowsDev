using Moq;
using WindowsDev.Business.Repositories.Interfaces;
using WindowsDev.Business.Services.TaskService.Comment;
using WindowsDev.Business.Services.UserManager;
using WindowsDev.Business.Services.UserManager.Interfaces;
using WindowsDev.Domain.TasksModels;

namespace WindowsDev.Tests.Business.TaskService.Comment
{
    public class CommentsServiceTest
    {
        private readonly Mock<ICommentRepository> _commentRepositoryMock;
        private readonly ICurrentUserService _currentUserService;

        public CommentsServiceTest()
        {
            _commentRepositoryMock = new Mock<ICommentRepository>();
            _currentUserService = new CurrentUserService();
            _currentUserService.Username = "testuser";
        }

        private CommentsService CreateService()
        {
            return new CommentsService(
                _commentRepositoryMock.Object,
                _currentUserService);
        }

        [Fact]
        public async Task AddComment_WhenTaskIdLessThan1_ThrowsException()
        {
            var service = CreateService();

            await Assert.ThrowsAsync<Exception>(
                () => service.AddComment(0, "comment text"));

            _commentRepositoryMock.Verify(x => x.AddComments(It.IsAny<Comments>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task AddComment_WhenCommentTextIsInvalid_ThrowsException(string commentText)
        {
            var service = CreateService();

            await Assert.ThrowsAsync<Exception>(
                () => service.AddComment(1, commentText!));

            _commentRepositoryMock.Verify(x => x.AddComments(It.IsAny<Comments>()), Times.Never);
        }

        [Fact]
        public async Task AddComment_WhenValid_SavesCommentAndReturnsIt()
        {
            var taskId = 1;
            var commentText = "test comment";
            var username = _currentUserService.Username;

            _commentRepositoryMock
                .Setup(x => x.AddComments(It.IsAny<Comments>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            var result = await service.AddComment(taskId, commentText);

            Assert.Equal(commentText, result.Text);
            Assert.Equal(username, result.Author);
            Assert.Equal(taskId, result.TaskId);
            Assert.True((DateTime.UtcNow - result.CreatedAt).TotalSeconds < 1);

            _commentRepositoryMock.Verify(x => x.AddComments(It.IsAny<Comments>()), Times.Once);
        }

        [Fact]
        public async Task GetComments_WhenCalled_ReturnsCommentsFromRepository()
        {
            var taskId = 1;
            var expectedComments = new List<Comments>
            {
                new Comments
                {
                    Id = 1,
                    Text = "comment1",
                    TaskId = taskId,
                    CreatedAt = DateTime.UtcNow,
                    Author = "user1"
                },
                new Comments
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
            _commentRepositoryMock.Verify(x => x.GetComments(taskId), Times.Once);
        }

        [Fact]
        public async Task GetComments_WhenNoComments_ReturnsEmptyList()
        {
            var taskId = 1;
            var expectedComments = new List<Comments>();

            _commentRepositoryMock
                .Setup(x => x.GetComments(taskId))
                .ReturnsAsync(expectedComments);

            var service = CreateService();

            var result = await service.GetComments(taskId);

            Assert.Empty(result);
            _commentRepositoryMock.Verify(x => x.GetComments(taskId), Times.Once);
        }
    }
}