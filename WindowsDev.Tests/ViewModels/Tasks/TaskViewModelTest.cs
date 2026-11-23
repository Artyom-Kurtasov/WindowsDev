using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using Moq;
using WindowsDev.ApiClient.AttachmentClient;
using WindowsDev.ApiClient.CommentClient;
using WindowsDev.Application.Common.Localization;
using WindowsDev.Application.Primitives;
using WindowsDev.Command;
using WindowsDev.Contracts.UI.Attachments;
using WindowsDev.Contracts.UI.Comments;
using WindowsDev.Contracts.UI.Projects;
using WindowsDev.Contracts.UI.Tasks;
using WindowsDev.Domain.Enums;
using WindowsDev.Domain.Messages;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;
using WindowsDev.Services.Dialogs;
using WindowsDev.Services.Navigation;
using WindowsDev.ViewModels.Tasks;
using WindowsDev.ViewModels.Tasks.Dialogs;
using WindowsDev.Views.Tasks;
using TaskStatus = WindowsDev.Domain.Enums.TaskStatus;

namespace WindowsDev.Tests.ViewModels.Tasks;

public class TaskViewModelTest
{
    private readonly Mock<ICommentApiClient> _commentApiClientMock;
    private readonly Mock<IDialogService> _dialogServiceMock;
    private readonly Mock<INavigationService> _navigationServiceMock;
    private readonly Mock<IAttachmentApiClient> _attachmentApiClientMock;
    private readonly Mock<ILogger<TaskViewModel>> _loggerMock;
    private readonly Mock<ILanguageChanger> _languageChangerMock;

    public TaskViewModelTest()
    {
        _commentApiClientMock = new();
        _dialogServiceMock = new();
        _navigationServiceMock = new();
        _attachmentApiClientMock = new();
        _loggerMock = new();
        _languageChangerMock = new();

        _languageChangerMock
            .Setup(x => x.Translate(It.IsAny<string>()))
            .Returns((string x) => x);
    }

    private TaskViewModel CreateViewModel(ProjectInfo? project = null, TaskInfo? task = null)
    {
        return new TaskViewModel(
            project ?? CreateProject(),
            task ?? CreateTask(),
            _commentApiClientMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object,
            _attachmentApiClientMock.Object,
            _loggerMock.Object,
            _languageChangerMock.Object
        );
    }

    private void SetupSuccessfulLoading(int taskId)
    {
        _commentApiClientMock
            .Setup(x => x.GetCommentsAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<CommentInfo>>.Success(new List<CommentInfo>()));

        _attachmentApiClientMock
            .Setup(x => x.GetFilesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<FileItem>>.Success(new List<FileItem>()));
    }

    private ProjectInfo CreateProject()
    {
        return new ProjectInfo(
            1,
            "Project",
            "Description",
            DateTime.UtcNow
        );
    }

    private TaskInfo CreateTask()
    {
        return new TaskInfo {
            Id = 1,
            Name = "Task",
            Status = TaskStatus.InProgress,
            Priority = TaskPriority.Medium,
            CreatedAt = DateTime.UtcNow,
            Deadline = DateTime.UtcNow.AddDays(7),
            ProjectId = 1,
            Description = "Description",
        };
    }

    private static async Task WaitUntilAsync(Func<bool> condition, int timeoutMs = 2000)
    {
        var start = Environment.TickCount;
        while (!condition())
        {
            if (Environment.TickCount - start > timeoutMs)
                throw new TimeoutException("Condition was not met in time.");

            await Task.Delay(10);
        }
    }

    [Fact]
    public async Task Constructor_WhenCalled_SetsTaskAndProject()
    {
        var project = CreateProject();
        var task = CreateTask();

        SetupSuccessfulLoading(task.Id);

        var vm = CreateViewModel(project, task);

        await WaitUntilAsync(() => vm.Comments is { Count: 0 } && vm.Attachments is { Count: 0 });

        Assert.Equal(project, vm.Project);
        Assert.Equal(task, vm.CurrentTask);
        Assert.Equal(task.Name, vm.Name);
    }

    [Fact]
    public async Task Constructor_WhenCommentsFail_ShowsError()
    {
        var task = CreateTask();

        _commentApiClientMock
            .Setup(x => x.GetCommentsAsync(task.Id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception());

        _attachmentApiClientMock
            .Setup(x => x.GetFilesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<FileItem>>.Success(new List<FileItem>()));

        var vm = CreateViewModel(task: task);

        await WaitUntilAsync(() => _dialogServiceMock.Invocations.Count > 0, 3000);

        _dialogServiceMock.Verify(
            x =>
                x.ShowMessageAsync(
                    DialogTitles.Error,
                    TaskErrors.LoadCommentsFailed,
                    MessageDialogStyle.Affirmative
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Constructor_WhenAttachmentsFail_ShowsError()
    {
        var task = CreateTask();

        _commentApiClientMock
            .Setup(x => x.GetCommentsAsync(task.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<CommentInfo>>.Success(new List<CommentInfo>()));

        _attachmentApiClientMock
            .Setup(x => x.GetFilesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception());

        var vm = CreateViewModel(task: task);

        await WaitUntilAsync(() => _dialogServiceMock.Invocations.Count > 0, 3000);

        _dialogServiceMock.Verify(
            x =>
                x.ShowMessageAsync(
                    DialogTitles.Error,
                    TaskErrors.LoadAttachmentsFailed,
                    MessageDialogStyle.Affirmative
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task AddComment_WhenSuccess_AddsCommentToCollection()
    {
        var task = CreateTask();

        SetupSuccessfulLoading(task.Id);

        var comment = new CommentInfo(1, "Test", DateTime.UtcNow, "User", task.Id);

        _commentApiClientMock
            .Setup(x => x.AddCommentAsync(It.IsAny<AddCommentForm>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CommentInfo>.Success(comment));

        var vm = CreateViewModel(task: task);

        await WaitUntilAsync(() => vm.Comments is { Count: 0 });

        vm.NewComment = "Test";

        await ((AsyncRelayCommand)vm.AddCommentCommand).ExecuteAsync(null);

        Assert.Equal(string.Empty, vm.NewComment);

        _commentApiClientMock.Verify(
            x =>
                x.AddCommentAsync(
                    It.Is<AddCommentForm>(f => f.TaskId == task.Id && f.Text == "Test"),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task AddComment_WhenEmpty_DoesNothing(string? text)
    {
        var task = CreateTask();

        SetupSuccessfulLoading(task.Id);

        var vm = CreateViewModel(task: task);

        vm.NewComment = text;

        await ((AsyncRelayCommand)vm.AddCommentCommand).ExecuteAsync(null);

        _commentApiClientMock.Verify(
            x => x.AddCommentAsync(It.IsAny<AddCommentForm>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task EditTaskCommand_ShowsDialog()
    {
        var task = CreateTask();

        SetupSuccessfulLoading(task.Id);

        var vm = CreateViewModel(task: task);

        await ((AsyncRelayCommand)vm.EditTaskCommand).ExecuteAsync(null);

        _dialogServiceMock.Verify(
            x => x.ShowDialogAsync<TaskDialogView, EditTaskViewModel>(task),
            Times.Once
        );
    }

    [Fact]
    public void OpenAttachment_WhenCalled_OpensFile()
    {
        var vm = CreateViewModel();

        const string filePath = @"C:\Temp\test.txt";

        ((AsyncRelayCommandT<string>)vm.OpenAttachmentCommand).ExecuteAsync(filePath)
            .GetAwaiter()
            .GetResult();

        _attachmentApiClientMock.Verify(
            x => x.DownloadFileAsync(filePath, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public void Dispose_WhenCalled_ClearsData()
    {
        var vm = CreateViewModel();

        vm.Dispose();

        Assert.Null(vm.Project);
        Assert.Null(vm.CurrentTask);
        Assert.Null(vm.Comments);
        Assert.Null(vm.Attachments);
    }

    [Fact]
    public void Dispose_WhenCalledMultipleTimes_DoesNotThrow()
    {
        var vm = CreateViewModel();

        vm.Dispose();

        var exception = Record.Exception(() => vm.Dispose());

        Assert.Null(exception);
    }
}