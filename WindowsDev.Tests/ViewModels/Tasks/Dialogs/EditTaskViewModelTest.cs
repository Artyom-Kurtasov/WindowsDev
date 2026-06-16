using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using Moq;
using WindowsDev.Business.Services.TaskService.Interfaces;
using WindowsDev.Domain.TasksModels;
using WindowsDev.Domain.TasksModels.Enums;
using WindowsDev.Infrastructure;
using WindowsDev.ViewModels.Tasks.Dialog;
using TaskStatus = WindowsDev.Domain.TasksModels.Enums.TaskStatus;

namespace WindowsDev.Tests.ViewModels.Tasks.Dialogs
{
    public class EditTaskViewModelTest
    {
        private readonly Mock<ITaskService> _taskServiceMock;
        private readonly Mock<IDialogCoordinator> _dialogCoordinatorMock;
        private readonly Mock<ILogger<EditTaskViewModel>> _loggerMock;

        private bool _completedEventWasRaised;
        private bool _closeEventWasRaised;

        public EditTaskViewModelTest()
        {
            _taskServiceMock = new Mock<ITaskService>();
            _dialogCoordinatorMock = new Mock<IDialogCoordinator>();
            _loggerMock = new Mock<ILogger<EditTaskViewModel>>();
        }

        private EditTaskViewModel CreateViewModel(TasksInfo task = null)
        {
            task ??= CreateTestTask();

            return new EditTaskViewModel(
                task,
                _taskServiceMock.Object,
                _dialogCoordinatorMock.Object,
                _loggerMock.Object);
        }

        private TasksInfo CreateTestTask(int id = 1, string name = "Test Task", string description = "Test Description")
        {
            return new TasksInfo
            {
                Id = id,
                Name = name,
                Description = description,
                ProjectId = 1,
                Priority = TaskPriority.Normal,
                Progress = 0,
                Status = TaskStatus.InProgress,
                DeadLine = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
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

        private void SetupDialogCoordinatorMock()
        {
            _dialogCoordinatorMock
                .Setup(x => x.ShowMessageAsync(
                    It.IsAny<EditTaskViewModel>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<MessageDialogStyle>()))
                .ReturnsAsync(MessageDialogResult.Affirmative);
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
            Assert.Equal(task.DeadLine, vm.DeadLine);
            Assert.True(vm.IsEditMode);
        }

        [Fact]
        public async Task EditTask_WhenSuccessful_UpdatesTask_RaisesEvents_ClosesDialog()
        {
            var task = CreateTestTask();
            var vm = CreateViewModel(task);
            SetupEvents(vm);

            vm.Name = "Updated Name";
            vm.Description = "Updated Description";
            vm.Priority = TaskPriority.Normal;
            vm.Progress = 50;
            vm.Status = TaskStatus.Done;

            _taskServiceMock
                .Setup(x => x.UpdateAsync(It.IsAny<TasksInfo>()))
                .Returns(Task.CompletedTask);

            await ((AsyncRelayCommand)vm.EditTaskCommand).ExecuteAsync(null);

            Assert.True(_completedEventWasRaised);
            Assert.True(_closeEventWasRaised);

            _taskServiceMock.Verify(x => x.UpdateAsync(It.Is<TasksInfo>(t =>
                t.Id == task.Id &&
                t.Name == "Updated Name" &&
                t.Description == "Updated Description" &&
                t.Priority == TaskPriority.Normal &&
                t.Progress == 50 &&
                t.Status == TaskStatus.Done)), Times.Once);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData(" ")]
        public async Task EditTask_WhenTaskNameIncorrect_ShowsWarningMessage(string? taskName)
        {
            var task = CreateTestTask();
            var vm = CreateViewModel(task);
            SetupEvents(vm);
            SetupDialogCoordinatorMock();

            vm.Name = taskName!;

            await ((AsyncRelayCommand)vm.EditTaskCommand).ExecuteAsync(null);

            Assert.False(_completedEventWasRaised);
            Assert.False(_closeEventWasRaised);

            _taskServiceMock
                .Verify(x => x.UpdateAsync(It.IsAny<TasksInfo>()), Times.Never);

            _dialogCoordinatorMock
                .Verify(x => x.ShowMessageAsync(
                    It.IsAny<EditTaskViewModel>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    MessageDialogStyle.Affirmative
                    ), Times.Once);
        }

        [Fact]
        public async Task EditTask_WhenExceptionOccurs_LogsErrorAndShowsErrorMessage()
        {
            var task = CreateTestTask();
            var vm = CreateViewModel(task);
            SetupEvents(vm);

            var loggerWasCalled = false;

            vm.Name = "Updated Name";

            _taskServiceMock
                .Setup(x => x.UpdateAsync(It.IsAny<TasksInfo>()))
                .ThrowsAsync(new Exception());

            SetupDialogCoordinatorMock();

            _loggerMock
                .Setup(x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Callback(() => loggerWasCalled = true);

            await ((AsyncRelayCommand)vm.EditTaskCommand).ExecuteAsync(null);

            Assert.False(_completedEventWasRaised);
            Assert.False(_closeEventWasRaised);
            Assert.True(loggerWasCalled);

            _taskServiceMock
                .Verify(x => x.UpdateAsync(It.IsAny<TasksInfo>()), Times.Once);

            _dialogCoordinatorMock
                .Verify(x => x.ShowMessageAsync(
                    It.IsAny<EditTaskViewModel>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    MessageDialogStyle.Affirmative
                    ), Times.Once);
        }
    }
}