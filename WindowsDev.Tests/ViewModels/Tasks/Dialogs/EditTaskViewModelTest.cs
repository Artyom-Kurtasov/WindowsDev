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

public class EditTaskViewModelTest
{
    private readonly Mock<ITaskApiClient> _apiClientMock;
    private readonly Mock<IDialogService> _dialogServiceMock;
    private readonly Mock<ILogger<EditTaskViewModel>> _loggerMock;
    private readonly Mock<ILanguageChanger> _languageChangerMock;

    private bool _completedEventWasRaised;
    private bool _closeEventWasRaised;

    public EditTaskViewModelTest()
    {
        _apiClientMock = new Mock<ITaskApiClient>();
        _dialogServiceMock = new Mock<IDialogService>();
        _loggerMock = new Mock<ILogger<EditTaskViewModel>>();
        _languageChangerMock = new Mock<ILanguageChanger>();

        _languageChangerMock
            .Setup(x => x.Translate(It.IsAny<string>()))
            .Returns((string key) => key);
    }

    private EditTaskViewModel CreateViewModel(TaskInfo? task = null)
    {
        task ??= CreateTestTask();

        return new EditTaskViewModel(
            task,
            _apiClientMock.Object,
            _dialogServiceMock.Object,
            _loggerMock.Object,
            _languageChangerMock.Object
        );
    }

    private TaskInfo CreateTestTask(
        int id = 1,
        string name = "Test Task",
        string description = "Test Description"
    )
    {
        return new TaskInfo {
            Id = id,
            Name = name,
            Description = description,
            Status = TaskStatus.InProgress,
            Priority = TaskPriority.Medium,
            CreatedAt = DateTime.UtcNow,
            Deadline = DateTime.UtcNow.AddDays(7),
            ProjectId = 1
        };
    }

    private void SetupEvents(EditTaskViewModel vm)
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
    public void Constructor_WhenTaskProvided_LoadsTaskData()
    {
        var task = CreateTestTask();

        var vm = CreateViewModel(task);

        Assert.Equal(task.Name, vm.Name);
        Assert.Equal(task.Description, vm.Description);
        Assert.Equal(task.Priority, vm.Priority);
        Assert.Equal(task.Progress, vm.Progress);
        Assert.Equal(task.Status, vm.Status);
        Assert.Equal(task.Deadline.ToLocalTime(), vm.DeadLine.ToLocalTime());
        Assert.True(vm.IsEditMode);
    }

    [Fact]
    public async Task SubmitCommand_WhenSuccessful_UpdatesTask_RaisesCompletedEvent()
    {
        var task = CreateTestTask();
        var vm = CreateViewModel(task);

        SetupEvents(vm);

        vm.Name = "Updated Name";
        vm.Description = "Updated Description";
        vm.Priority = TaskPriority.Medium;
        vm.Progress = 50;
        vm.Status = TaskStatus.Completed;

        UpdateTaskForm? capturedForm = null;
        int capturedId = 0;

        _apiClientMock
            .Setup(x => x.UpdateAsync(
                It.IsAny<int>(),
                It.IsAny<UpdateTaskForm>(),
                It.IsAny<CancellationToken>()))
            .Callback<int, UpdateTaskForm, CancellationToken>(
                (id, form, _) =>
                {
                    capturedId = id;
                    capturedForm = form;
                }
            )
            .ReturnsAsync(Result<UpdateTaskInfo>.Success(new UpdateTaskInfo
            (
                task.Id,
                task.Name,
                task.Description,
                task.Priority,
                task.Status,
                task.Deadline,
                task.CreatedAt,
                task.Progress,
                task.ProjectId
            )));

        await ((AsyncRelayCommand)vm.SubmitCommand).ExecuteAsync(null);

        Assert.True(_completedEventWasRaised);
        Assert.True(_closeEventWasRaised);

        Assert.Equal(task.Id, capturedId);
        Assert.NotNull(capturedForm);
        Assert.Equal("Updated Name", capturedForm.Name);
        Assert.Equal("Updated Description", capturedForm.Description);
        Assert.Equal(TaskPriority.Medium, capturedForm.Priority);
        Assert.Equal(50, capturedForm.Progress);
        Assert.Equal(TaskStatus.Completed, capturedForm.Status);

        _apiClientMock.Verify(
            x => x.UpdateAsync(
                It.IsAny<int>(),
                It.IsAny<UpdateTaskForm>(),
                It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData(" ")]
    public async Task SubmitCommand_WhenNameEmpty_ShowsWarning(string? taskName)
    {
        var task = CreateTestTask();
        var vm = CreateViewModel(task);

        SetupEvents(vm);

        vm.Name = taskName;

        await ((AsyncRelayCommand)vm.SubmitCommand).ExecuteAsync(null);

        Assert.False(_completedEventWasRaised);
        Assert.False(_closeEventWasRaised);

        _apiClientMock.Verify(
            x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<UpdateTaskForm>()),
            Times.Never
        );

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
    public async Task SubmitCommand_WhenExceptionOccurs_ShowsErrorDialog()
    {
        var task = CreateTestTask();
        var vm = CreateViewModel(task);

        SetupEvents(vm);

        _apiClientMock
            .Setup(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<UpdateTaskForm>()))
            .ThrowsAsync(new Exception());

        await ((AsyncRelayCommand)vm.SubmitCommand).ExecuteAsync(null);

        Assert.False(_completedEventWasRaised);
        Assert.False(_closeEventWasRaised);

        _apiClientMock.Verify(
            x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<UpdateTaskForm>()),
            Times.Once
        );

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

    [Fact]
    public async Task CancelCommand_WhenExecuted_RaisesCloseEventAndDisablesEditMode()
    {
        var vm = CreateViewModel();

        SetupEvents(vm);

        Assert.True(vm.IsEditMode);

        await ((AsyncRelayCommand)vm.CancelCommand).ExecuteAsync(null);

        Assert.True(_closeEventWasRaised);
        Assert.False(vm.IsEditMode);
    }
}