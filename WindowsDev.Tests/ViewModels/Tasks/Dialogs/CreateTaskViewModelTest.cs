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

        private CreateTaskViewModel CreateViewModel()
        {
            return new CreateTaskViewModel(
                _taskServiceMock.Object,
                _dialogCoordinatorMock.Object,
                _loggerMock.Object);
        }

        private void SetupViewModel(
            CreateTaskViewModel vm,
            string name,
            string description,
            int projectId)
        {
            vm.Name = name;
            vm.Description = description;
            vm.Priority = TaskPriority.Normal;
            vm.Progress = 0;
            vm.Status = TaskStatus.InProgress;
            vm.DeadLine = DateTime.UtcNow.AddDays(7);

            var initializationTask = vm.InitializationAsync(projectId);
            initializationTask.Wait();
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
                    It.IsAny<string>()))
                .ReturnsAsync(MessageDialogResult.Affirmative);
        }

        [Fact]
        public async Task InitializationAsync_WhenProjectIdLessThanOne_ThrowsArgumentNullException()
        {
            var vm = CreateViewModel();

            await Assert.ThrowsAsync<ArgumentNullException>(
                () => vm.InitializationAsync(0));
        }

        [Fact]
        public async Task InitializationAsync_WhenNoArguments_ThrowsArgumentNullException()
        {
            var vm = CreateViewModel();

            await Assert.ThrowsAsync<ArgumentNullException>(
                () => vm.InitializationAsync());
        }

        [Fact]
        public async Task CancelCommand_WhenExecuted_RaisesCloseRequestedEvent()
        {
            var vm = CreateViewModel();
            SetupEvents(vm);
            await vm.InitializationAsync(1);

            await ((AsyncRelayCommand)vm.CancelCommand).ExecuteAsync(null);

            Assert.True(_closeEventWasRaised);
            Assert.False(_completedEventWasRaised);
        }

        [Fact]
        public async Task CreateTask_WhenSuccessful_AddsTask_RaisesEvents_ClosesDialog()
        {
            var vm = CreateViewModel();
            SetupViewModel(vm, "Test Task", "Test Description", 1);
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
            SetupViewModel(vm, taskName!, "Test Description", 1);
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
            SetupViewModel(vm, "Test Task", "Test Description", 1);
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