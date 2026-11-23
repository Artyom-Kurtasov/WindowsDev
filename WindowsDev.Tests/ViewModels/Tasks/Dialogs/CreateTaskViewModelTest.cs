using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using Moq;
using WindowsDev.ApiClient.TaskClient;
using WindowsDev.Application.Common.Localization;
using WindowsDev.Application.Primitives;
using WindowsDev.Command;
using WindowsDev.Contracts.UI.Tasks;
using WindowsDev.Domain.Enums;
using WindowsDev.Domain.Messages;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;
using WindowsDev.Domain.Messages.DialogsMessages.Warnings;
using WindowsDev.Services.Dialogs;
using WindowsDev.ViewModels.Tasks.Dialogs;
using TaskStatus = WindowsDev.Domain.Enums.TaskStatus;

namespace WindowsDev.Tests.ViewModels.Tasks.Dialogs;

public class CreateTaskViewModelTest
{
    private readonly Mock<ITaskApiClient> _apiClientMock;
    private readonly Mock<IDialogService> _dialogServiceMock;
    private readonly Mock<ILogger<CreateTaskViewModel>> _loggerMock;
    private readonly Mock<ILanguageChanger> _languageChangerMock;

    private bool _completedEventWasRaised;
    private bool _closeEventWasRaised;

    public CreateTaskViewModelTest()
    {
        _apiClientMock = new Mock<ITaskApiClient>();
        _dialogServiceMock = new Mock<IDialogService>();
        _loggerMock = new Mock<ILogger<CreateTaskViewModel>>();
        _languageChangerMock = new Mock<ILanguageChanger>();

        _languageChangerMock
            .Setup(x => x.Translate(It.IsAny<string>()))
            .Returns((string key) => key);
    }

    private CreateTaskViewModel CreateViewModel(int projectId = 1)
    {
        var vm = new CreateTaskViewModel(
            projectId,
            _apiClientMock.Object,
            _dialogServiceMock.Object,
            _loggerMock.Object,
            _languageChangerMock.Object
        );

        vm.Name = "Test Task";
        vm.Description = "Test Description";
        vm.Priority = TaskPriority.Medium;
        vm.Progress = 0;
        vm.Status = TaskStatus.InProgress;
        vm.DeadLine = DateTime.UtcNow.AddDays(7);

        return vm;
    }

    private void SetupEvents(CreateTaskViewModel vm)
    {
        _completedEventWasRaised = false;
        _closeEventWasRaised = false;

        vm.Completed += () =>
        {
            _completedEventWasRaised = true;
            return Task.CompletedTask;
        };

        vm.CloseRequested += () =>
        {
            _closeEventWasRaised = true;
            return Task.CompletedTask;
        };
    }

    [Fact]
    public void Constructor_WhenProjectIdLessThanOne_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CreateTaskViewModel(
                0,
                _apiClientMock.Object,
                _dialogServiceMock.Object,
                _loggerMock.Object,
                _languageChangerMock.Object
            )
        );
    }

    [Fact]
    public async Task CancelCommand_WhenExecuted_RaisesCloseRequestedEvent()
    {
        var vm = CreateViewModel();

        SetupEvents(vm);

        await ((AsyncRelayCommand)vm.CancelCommand).ExecuteAsync(null);

        Assert.True(_closeEventWasRaised);
        Assert.False(_completedEventWasRaised);
    }

    [Fact]
    public async Task SubmitCommand_WhenSuccessful_AddsTask_RaisesCompletedAndCloseEvents()
    {
        var vm = CreateViewModel();

        SetupEvents(vm);

        TaskForm? capturedForm = null;

        _apiClientMock
            .Setup(x => x.AddAsync(It.IsAny<TaskForm>()))
            .Callback<TaskForm, CancellationToken>((form, _) => capturedForm = form)
            .ReturnsAsync(Result<TaskInfo>.Success(It.IsAny<TaskInfo>()));

        await ((AsyncRelayCommand)vm.SubmitCommand).ExecuteAsync(null);

        Assert.True(_completedEventWasRaised);
        Assert.True(_closeEventWasRaised);

        Assert.NotNull(capturedForm);

        Assert.Equal("Test Task", capturedForm.Name);
        Assert.Equal("Test Description", capturedForm.Description);
        Assert.Equal(1, capturedForm.ProjectId);
        Assert.Equal(TaskPriority.Medium, capturedForm.Priority);
        Assert.Equal(TaskStatus.InProgress, capturedForm.Status);
        Assert.Equal(0, capturedForm.Progress);

        _apiClientMock.Verify(x => x.AddAsync(It.IsAny<TaskForm>()), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData(" ")]
    public async Task SubmitCommand_WhenNameEmpty_ShowsWarning(string? name)
    {
        var vm = CreateViewModel();

        vm.Name = name;

        SetupEvents(vm);

        await ((AsyncRelayCommand)vm.SubmitCommand).ExecuteAsync(null);

        Assert.False(_completedEventWasRaised);
        Assert.False(_closeEventWasRaised);

        _apiClientMock.Verify(x => x.AddAsync(It.IsAny<TaskForm>()), Times.Never);

        _dialogServiceMock.Verify(
            x =>
                x.ShowMessageAsync(
                    DialogTitles.Warning,
                    TaskDialogWarnings.EnterName,
                    MessageDialogStyle.Affirmative
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task SubmitCommand_WhenServiceThrows_ShowsErrorDialog()
    {
        var vm = CreateViewModel();

        SetupEvents(vm);

        _apiClientMock
            .Setup(x => x.AddAsync(It.IsAny<TaskForm>()))
            .ThrowsAsync(new Exception());

        await ((AsyncRelayCommand)vm.SubmitCommand).ExecuteAsync(null);

        Assert.False(_completedEventWasRaised);
        Assert.False(_closeEventWasRaised);

        _apiClientMock.Verify(x => x.AddAsync(It.IsAny<TaskForm>()), Times.Once);

        _dialogServiceMock.Verify(
            x =>
                x.ShowMessageAsync(
                    DialogTitles.Error,
                    CommonErrors.UnexpectedError,
                    MessageDialogStyle.Affirmative
                ),
            Times.Once
        );
    }
}