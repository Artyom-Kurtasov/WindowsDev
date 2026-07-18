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
    public class EditTaskViewModelTest
    {
        private readonly Mock<ITaskService> _taskServiceMock;
        private readonly Mock<IDialogCoordinator> _dialogCoordinatorMock;
        private readonly Mock<ILogger<EditTaskViewModel>> _loggerMock;
        private readonly Mock<ILanguageChanger> _languageChangerMock;

        private bool _completedEventWasRaised;
        private bool _closeEventWasRaised;

        public EditTaskViewModelTest()
        {
            _taskServiceMock = new Mock<ITaskService>();
            _dialogCoordinatorMock = new Mock<IDialogCoordinator>();
            _loggerMock = new Mock<ILogger<EditTaskViewModel>>();
            _languageChangerMock = new Mock<ILanguageChanger>();

            _languageChangerMock
                .Setup(x => x.Translate(It.IsAny<string>()))
                .Returns((string key) => key);
        }

        private EditTaskViewModel CreateViewModel(TasksInfo? task = null)
        {
            task ??= CreateTestTask();

            return new EditTaskViewModel(
                task,
                _taskServiceMock.Object,
                _dialogCoordinatorMock.Object,
                _loggerMock.Object,
                _languageChangerMock.Object
            );
        }

        private TasksInfo CreateTestTask(
            int id = 1,
            string name = "Test Task",
            string description = "Test Description"
        )
        {
            return new TasksInfo
            {
                Id = id,
                Name = name,
                Description = description,
                ProjectId = 1,
                Priority = TaskPriority.Medium,
                Progress = 0,
                Status = TaskStatus.InProgress,
                DeadLine = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
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
            Assert.Equal(task.DeadLine.ToLocalTime(), vm.DeadLine.ToLocalTime());
            Assert.True(vm.IsEditMode);
        }

        [Fact]
        public async Task EditTask_WhenSuccessful_UpdatesTask_RaisesCompletedEvent()
        {
            var task = CreateTestTask();
            var vm = CreateViewModel(task);

            SetupEvents(vm);

            vm.Name = "Updated Name";
            vm.Description = "Updated Description";
            vm.Priority = TaskPriority.Medium;
            vm.Progress = 50;
            vm.Status = TaskStatus.Completed;

            _taskServiceMock
                .Setup(x => x.UpdateAsync(It.IsAny<TasksInfo>()))
                .Returns(Task.CompletedTask);

            await ((AsyncRelayCommand)vm.EditTaskCommand).ExecuteAsync(null);

            Assert.True(_completedEventWasRaised);
            Assert.True(_closeEventWasRaised);

            _taskServiceMock.Verify(
                x =>
                    x.UpdateAsync(
                        It.Is<TasksInfo>(t =>
                            t.Id == task.Id
                            && t.Name == "Updated Name"
                            && t.Description == "Updated Description"
                            && t.Priority == TaskPriority.Medium
                            && t.Progress == 50
                            && t.Status == TaskStatus.Completed
                        )
                    ),
                Times.Once
            );
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData(" ")]
        public async Task EditTask_WhenNameEmpty_ShowsWarning(string? taskName)
        {
            var task = CreateTestTask();
            var vm = CreateViewModel(task);

            SetupEvents(vm);

            vm.Name = taskName;

            await ((AsyncRelayCommand)vm.EditTaskCommand).ExecuteAsync(null);

            Assert.False(_completedEventWasRaised);
            Assert.False(_closeEventWasRaised);

            _taskServiceMock.Verify(x => x.UpdateAsync(It.IsAny<TasksInfo>()), Times.Never);

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
        public async Task EditTask_WhenExceptionOccurs_ShowsErrorDialog()
        {
            var task = CreateTestTask();
            var vm = CreateViewModel(task);

            SetupEvents(vm);

            _taskServiceMock
                .Setup(x => x.UpdateAsync(It.IsAny<TasksInfo>()))
                .ThrowsAsync(new Exception());

            await ((AsyncRelayCommand)vm.EditTaskCommand).ExecuteAsync(null);

            Assert.False(_completedEventWasRaised);
            Assert.False(_closeEventWasRaised);

            _taskServiceMock.Verify(x => x.UpdateAsync(It.IsAny<TasksInfo>()), Times.Once);

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

        [Fact]
        public async Task CloseCommand_WhenExecuted_RaisesCloseEventAndDisablesEditMode()
        {
            var vm = CreateViewModel();

            SetupEvents(vm);

            Assert.True(vm.IsEditMode);

            await ((AsyncRelayCommand)vm.CancelCommand).ExecuteAsync(null);

            Assert.True(_closeEventWasRaised);
            Assert.False(vm.IsEditMode);
        }
    }
}
