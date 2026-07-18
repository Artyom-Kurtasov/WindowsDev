using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using Moq;
using WindowsDev.Business.Services.Localization.Interfaces;
using WindowsDev.Business.Services.TaskService.Interfaces;
using WindowsDev.Domain;
using WindowsDev.Domain.DialogsMessages.Errors;
using WindowsDev.Domain.DialogsMessages.Warnings;
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
        private readonly Mock<ILanguageChanger> _languageChangerMock;

        private bool _completedEventWasRaised;
        private bool _closeEventWasRaised;

        public CreateTaskViewModelTest()
        {
            _taskServiceMock = new Mock<ITaskService>();
            _dialogCoordinatorMock = new Mock<IDialogCoordinator>();
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
                _taskServiceMock.Object,
                _dialogCoordinatorMock.Object,
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
                    _taskServiceMock.Object,
                    _dialogCoordinatorMock.Object,
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
        public async Task CreateTask_WhenSuccessful_AddsTask_RaisesCompletedAndCloseEvents()
        {
            var vm = CreateViewModel();

            SetupEvents(vm);

            TasksInfo? createdTask = null;

            _taskServiceMock
                .Setup(x => x.AddAsync(It.IsAny<TasksInfo>()))
                .Callback<TasksInfo>(task => createdTask = task)
                .Returns(Task.CompletedTask);

            await ((AsyncRelayCommand)vm.CreateTaskCommand).ExecuteAsync(null);

            Assert.True(_completedEventWasRaised);
            Assert.True(_closeEventWasRaised);

            Assert.NotNull(createdTask);

            Assert.Equal("Test Task", createdTask.Name);
            Assert.Equal("Test Description", createdTask.Description);
            Assert.Equal(1, createdTask.ProjectId);
            Assert.Equal(TaskPriority.Medium, createdTask.Priority);
            Assert.Equal(TaskStatus.InProgress, createdTask.Status);

            _taskServiceMock.Verify(x => x.AddAsync(It.IsAny<TasksInfo>()), Times.Once);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData(" ")]
        public async Task CreateTask_WhenNameEmpty_ShowsWarning(string? name)
        {
            var vm = CreateViewModel();

            vm.Name = name;

            SetupEvents(vm);

            await ((AsyncRelayCommand)vm.CreateTaskCommand).ExecuteAsync(null);

            Assert.False(_completedEventWasRaised);
            Assert.False(_closeEventWasRaised);

            _taskServiceMock.Verify(x => x.AddAsync(It.IsAny<TasksInfo>()), Times.Never);

            _dialogCoordinatorMock.Verify(
                x =>
                    x.ShowMessageAsync(
                        vm,
                        DialogTitles.Warning,
                        TaskDialogWarnings.EnterName,
                        MessageDialogStyle.Affirmative
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task CreateTask_WhenServiceThrows_ShowsErrorDialog()
        {
            var vm = CreateViewModel();

            SetupEvents(vm);

            _taskServiceMock
                .Setup(x => x.AddAsync(It.IsAny<TasksInfo>()))
                .ThrowsAsync(new Exception());

            await ((AsyncRelayCommand)vm.CreateTaskCommand).ExecuteAsync(null);

            Assert.False(_completedEventWasRaised);
            Assert.False(_closeEventWasRaised);

            _taskServiceMock.Verify(x => x.AddAsync(It.IsAny<TasksInfo>()), Times.Once);

            _dialogCoordinatorMock.Verify(
                x =>
                    x.ShowMessageAsync(
                        vm,
                        DialogTitles.Error,
                        CommonErrors.UnexpectedError,
                        MessageDialogStyle.Affirmative
                    ),
                Times.Once
            );
        }
    }
}
