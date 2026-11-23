using Moq;
using WindowsDev.Application.Identity;
using WindowsDev.Application.Tasks.Comment;
using WindowsDev.Domain.Entities;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;

namespace WindowsDev.Tests.Business.TaskService.Comment;

public class CommentsServiceTest
{
    private readonly Mock<ICommentRepository> _commentRepositoryMock;
    private readonly Mock<IUserSession> _userSessionMock;

    public CommentsServiceTest()
    {
        _commentRepositoryMock = new Mock<ICommentRepository>();
        _userSessionMock = new Mock<IUserSession>();
        _userSessionMock.SetupGet(x => x.Username).Returns("testuser");
    }

    private WindowsDev.Application.Tasks.Comment.CommentsService CreateService() =>
        new WindowsDev.Application.Tasks.Comment.CommentsService(
            _commentRepositoryMock.Object,
            _userSessionMock.Object
        );

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task AddComment_WhenTaskIdLessThan1_ThrowsArgumentOutOfRangeException(int taskId)
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            service.AddCommentAsync(taskId, "comment text")
        );

        _commentRepositoryMock.Verify(
            x => x.AddComments(It.IsAny<TaskComment>()),
            Times.Never
        );
    }

    [Fact]
    public async Task AddComment_WhenValid_SavesCommentAndReturnsIt()
    {
        const int taskId = 1;
        const string commentText = "test comment";

        var service = CreateService();

        var result = await service.AddCommentAsync(taskId, commentText);

        Assert.Equal(commentText, result.Text);
        Assert.Equal("testuser", result.Author);
        Assert.Equal(taskId, result.TaskId);
        Assert.True((DateTime.UtcNow - result.CreatedAt).TotalSeconds < 1);

        _commentRepositoryMock.Verify(
            x => x.AddComments(It.Is<TaskComment>(c =>
                c.Text == commentText
                && c.Author == "testuser"
                && c.TaskId == taskId
            )),
            Times.Once
        );
    }

    [Fact]
    public async Task GetComments_WhenCalled_ReturnsCommentsFromRepository()
    {
        const int taskId = 1;

        var expectedComments = new List<TaskComment>
        {
            new()
            {
                Id = 1,
                Text = "comment1",
                TaskId = taskId,
                CreatedAt = DateTime.UtcNow,
                Author = "user1",
            },
            new()
            {
                Id = 2,
                Text = "comment2",
                TaskId = taskId,
                CreatedAt = DateTime.UtcNow,
                Author = "user2",
            },
        };

        _commentRepositoryMock
            .Setup(x => x.GetComments(taskId))
            .ReturnsAsync(expectedComments);

        var service = CreateService();

        var result = await service.GetCommentsAsync(taskId);

        Assert.Same(expectedComments, result);
        Assert.Equal(2, result.Count);

        _commentRepositoryMock.Verify(x => x.GetComments(taskId), Times.Once);
    }

    [Fact]
    public async Task GetComments_WhenNoComments_ReturnsEmptyList()
    {
        const int taskId = 1;

        _commentRepositoryMock
            .Setup(x => x.GetComments(taskId))
            .ReturnsAsync(new List<TaskComment>());

        var service = CreateService();

        var result = await service.GetCommentsAsync(taskId);

        Assert.Empty(result);

        _commentRepositoryMock.Verify(x => x.GetComments(taskId), Times.Once);
    }

    [Fact]
    public async Task GetCommentAsync_WhenExists_ReturnsSuccess()
    {
        const int commentId = 7;
        var comment = new TaskComment
        {
            Id = commentId,
            Text = "text",
            TaskId = 1,
            Author = "user",
            CreatedAt = DateTime.UtcNow,
        };

        _commentRepositoryMock
            .Setup(x => x.GetComment(commentId))
            .ReturnsAsync(comment);

        var service = CreateService();

        var result = await service.GetCommentAsync(commentId);

        Assert.True(result.IsSuccess);
        Assert.Same(comment, result.Value);

        _commentRepositoryMock.Verify(x => x.GetComment(commentId), Times.Once);
    }

    [Fact]
    public async Task GetCommentAsync_WhenMissing_ReturnsFailure()
    {
        _commentRepositoryMock
            .Setup(x => x.GetComment(It.IsAny<int>()))
            .ReturnsAsync((TaskComment?)null);

        var service = CreateService();

        var result = await service.GetCommentAsync(1);

        Assert.False(result.IsSuccess);
        Assert.Equal(CommentErrors.CommentNotFound, result.Error);
    }
}