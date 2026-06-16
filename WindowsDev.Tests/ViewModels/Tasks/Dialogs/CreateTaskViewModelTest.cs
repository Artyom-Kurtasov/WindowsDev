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
    public class CreateTaskViewModelTest
    {
        private readonly Mock<ITaskService> _taskServiceMock;
        private readonly Mock<IDialogCoordinator> _dialogCoordinatorMock;
        private readonly Mock<ILogger<CreateTaskViewModel>> _loggerMock;

        private bool _completedEventWasRaised;
        private bool _closeEventWasRaised;

        public CreateTaskViewModelTest()
        {
            _taskServiceMock = new Mock<ITaskService>();
            _dialogCoordinatorMock = new Mock<IDialogCoordinator>();
            _loggerMock = new Mock<ILogger<CreateTaskViewModel>>();
        }

        private CreateTaskViewModel CreateViewModel(int projectId = 1)
        {
            var vm = new CreateTaskViewModel(
                projectId,
                _taskServiceMock.Object,
                _dialogCoordinatorMock.Object,
                _loggerMock.Object);

            vm.Name = "Test Task";
            vm.Description = "Test Description";
            vm.Priority = TaskPriority.Normal;
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

        private void SetupDialogCoordinatorMock()
        {
            _dialogCoordinatorMock
                .Setup(x => x.ShowMessageAsync(
                    It.IsAny<CreateTaskViewModel>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<MessageDialogStyle>()))
                .ReturnsAsync(MessageDialogResult.Affirmative);
        }

        [Fact]
        public void Constructor_WhenProjectIdLessThanOne_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CreateTaskViewModel(
                0,
                _taskServiceMock.Object,
                _dialogCoordinatorMock.Object,
                _loggerMock.Object));
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
        public async Task CreateTask_WhenSuccessful_AddsTask_RaisesEvents_ClosesDialog()
        {
            var vm = CreateViewModel();
            SetupEvents(vm);

            _taskServiceMock
                .Setup(x => x.AddAsync(It.IsAny<TasksInfo>()))
                .Returns(Task.CompletedTask);

            await ((AsyncRelayCommand)vm.CreateTaskCommand).ExecuteAsync(null);

            Assert.True(_completedEventWasRaised);
            Assert.True(_closeEventWasRaised);

            _taskServiceMock.Verify(x => x.AddAsync(It.Is<TasksInfo>(t =>
                t.Name == "Test Task" &&
                t.Description == "Test Description" &&
                t.ProjectId == 1 &&
                t.CreatedAt.Date == DateTime.UtcNow.Date)), Times.Once);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData(" ")]
        public async Task CreateTask_WhenTaskNameIncorrect_ShowsWarningMessage(string? taskName)
        {
            var vm = CreateViewModel();
            vm.Name = taskName;
            SetupEvents(vm);
            SetupDialogCoordinatorMock();

            await ((AsyncRelayCommand)vm.CreateTaskCommand).ExecuteAsync(null);

            Assert.False(_completedEventWasRaised);
            Assert.False(_closeEventWasRaised);

            _taskServiceMock
                .Verify(x => x.AddAsync(It.IsAny<TasksInfo>()), Times.Never);

            _dialogCoordinatorMock
                .Verify(x => x.ShowMessageAsync(
                    It.IsAny<CreateTaskViewModel>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    MessageDialogStyle.Affirmative
                    ), Times.Once);
        }

        [Fact]
        public async Task CreateTask_WhenExceptionOccurs_LogsErrorAndShowsErrorMessage()
        {
            var vm = CreateViewModel();
            SetupEvents(vm);

            var loggerWasCalled = false;

            _taskServiceMock
                .Setup(x => x.AddAsync(It.IsAny<TasksInfo>()))
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

            await ((AsyncRelayCommand)vm.CreateTaskCommand).ExecuteAsync(null);

            Assert.False(_completedEventWasRaised);
            Assert.False(_closeEventWasRaised);
            Assert.True(loggerWasCalled);

            _taskServiceMock
                .Verify(x => x.AddAsync(It.IsAny<TasksInfo>()), Times.Once);

            _dialogCoordinatorMock
                .Verify(x => x.ShowMessageAsync(
                    It.IsAny<CreateTaskViewModel>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    MessageDialogStyle.Affirmative
                    ), Times.Once);
        }
    }
}