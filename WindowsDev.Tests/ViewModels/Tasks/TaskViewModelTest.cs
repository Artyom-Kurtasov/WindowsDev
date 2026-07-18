using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using Moq;
using WindowsDev.Business.Primitives;
using WindowsDev.Business.Services.Localization.Interfaces;
using WindowsDev.Business.Services.TaskService.Attachment.Interfaces;
using WindowsDev.Business.Services.TaskService.Comment.Interfaces;
using WindowsDev.Dialogs.Interfaces;
using WindowsDev.Domain;
using WindowsDev.Domain.DialogsMessages.Errors;
using WindowsDev.Domain.ProjectsModels;
using WindowsDev.Domain.TasksModels;
using WindowsDev.Domain.TasksModels.Enums;
using WindowsDev.Infrastructure;
using WindowsDev.NavigationManager.Interfaces;
using WindowsDev.ViewModels.Tasks;
using WindowsDev.ViewModels.Tasks.Dialog;
using WindowsDev.Views.Tasks;
using TaskStatus = WindowsDev.Domain.TasksModels.Enums.TaskStatus;

namespace WindowsDev.Tests.ViewModels.Tasks
{
    public class TaskViewModelTest
    {
        private readonly Mock<ICommentService> _commentServiceMock;
        private readonly Mock<IDialogService> _dialogServiceMock;
        private readonly Mock<INavigationService> _navigationServiceMock;
        private readonly Mock<IAttacmentService> _attachmentServiceMock;
        private readonly Mock<ILogger<TaskViewModel>> _loggerMock;
        private readonly Mock<IDialogCoordinator> _dialogCoordinatorMock;
        private readonly Mock<ILanguageChanger> _languageChangerMock;

        public TaskViewModelTest()
        {
            _commentServiceMock = new();
            _dialogServiceMock = new();
            _navigationServiceMock = new();
            _attachmentServiceMock = new();
            _loggerMock = new();
            _dialogCoordinatorMock = new();
            _languageChangerMock = new();

            _languageChangerMock
                .Setup(x => x.Translate(It.IsAny<string>()))
                .Returns((string x) => x);
        }

        private TaskViewModel CreateViewModel(ProjectsInfo? project = null, TasksInfo? task = null)
        {
            return new TaskViewModel(
                project ?? CreateProject(),
                task ?? CreateTask(),
                _commentServiceMock.Object,
                _dialogServiceMock.Object,
                _navigationServiceMock.Object,
                _attachmentServiceMock.Object,
                _loggerMock.Object,
                _dialogCoordinatorMock.Object,
                _languageChangerMock.Object
            );
        }

        private void SetupSuccessfulLoading(int taskId)
        {
            _commentServiceMock
                .Setup(x => x.GetComments(taskId))
                .ReturnsAsync(new List<Comments>());

            _attachmentServiceMock
                .Setup(x => x.GetAttachmentsAsync(taskId))
                .ReturnsAsync(new List<TaskAttachment>());
        }

        private ProjectsInfo CreateProject()
        {
            return new()
            {
                Id = 1,
                Name = "Project",
                UserId = 1,
                CreatedAt = DateTime.UtcNow,
            };
        }

        private TasksInfo CreateTask()
        {
            return new()
            {
                Id = 1,
                Name = "Task",
                Description = "Description",
                ProjectId = 1,
                Priority = TaskPriority.Medium,
                Status = TaskStatus.InProgress,
                Progress = 0,
                CreatedAt = DateTime.UtcNow,
                DeadLine = DateTime.UtcNow.AddDays(7),
            };
        }

        [Fact]
        public void Constructor_WhenCalled_SetsTaskAndProject()
        {
            var project = CreateProject();
            var task = CreateTask();

            SetupSuccessfulLoading(task.Id);

            var vm = CreateViewModel(project, task);

            Assert.Equal(project, vm.Project);
            Assert.Equal(task, vm.CurrentTask);
            Assert.Equal(task.Name, vm.Name);
        }

        [Fact]
        public async Task Constructor_WhenCommentsFail_ShowsError()
        {
            var task = CreateTask();

            _commentServiceMock.Setup(x => x.GetComments(task.Id)).ThrowsAsync(new Exception());

            var vm = CreateViewModel(task: task);

            await Task.Delay(50);

            _dialogCoordinatorMock.Verify(
                x =>
                    x.ShowMessageAsync(
                        vm,
                        DialogTitles.Error,
                        TaskErrors.LoadCommentsFailed,
                        MessageDialogStyle.Affirmative,
                        It.IsAny<MetroDialogSettings>()
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task Constructor_WhenAttachmentsFail_ShowsError()
        {
            var task = CreateTask();

            _attachmentServiceMock
                .Setup(x => x.GetAttachmentsAsync(task.Id))
                .ThrowsAsync(new Exception());

            var vm = CreateViewModel(task: task);

            await Task.Delay(50);

            _dialogCoordinatorMock.Verify(
                x =>
                    x.ShowMessageAsync(
                        vm,
                        DialogTitles.Error,
                        TaskErrors.LoadAttachmentsFailed,
                        MessageDialogStyle.Affirmative,
                        It.IsAny<MetroDialogSettings>()
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task AddComment_WhenSuccess_AddsCommentToCollection()
        {
            var task = CreateTask();

            SetupSuccessfulLoading(task.Id);

            var comment = new Comments
            {
                Id = 1,
                Text = "Test",
                CreatedAt = DateTime.UtcNow,
                Author = "User",
                TaskId = task.Id,
                Task = task,
            };

            _commentServiceMock
                .Setup(x => x.AddComment(task.Id, "Test"))
                .ReturnsAsync(Result<Comments>.Success(comment));

            var vm = CreateViewModel(task: task);

            await Task.Delay(50);

            vm.NewComment = "Test";

            await ((AsyncRelayCommand)vm.AddCommentCommand).ExecuteAsync(null);

            Assert.Contains(comment, vm.Comments!);
            Assert.Equal(string.Empty, vm.NewComment);
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

            _commentServiceMock.Verify(
                x => x.AddComment(It.IsAny<int>(), It.IsAny<string>()),
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
                x => x.ShowDialogAsync<TaskDialogView, EditTaskViewModel>(vm, task),
                Times.Once
            );
        }

        [Fact]
        public async Task AddAttachment_WhenSuccess_AddsAttachment()
        {
            var task = CreateTask();

            SetupSuccessfulLoading(task.Id);

            var attachment = new TaskAttachment
            {
                Id = 1,
                FileName = "test.txt",
                FileExtension = ".txt",
                FilePath = @"C:\Temp\test.txt",
                FileSize = 1024,
                TaskId = task.Id,
                Task = task,
            };

            _attachmentServiceMock
                .Setup(x => x.AddFile(task.Id))
                .ReturnsAsync(Result<TaskAttachment>.Success(attachment));

            var vm = CreateViewModel(task: task);

            await ((AsyncRelayCommand)vm.AddAttachmentCommand).ExecuteAsync(null);

            Assert.Contains(attachment, vm.Attachments!);
        }

        [Fact]
        public async Task OpenAttachment_WhenCalled_OpensFile()
        {
            var attachment = new TaskAttachment
            {
                Id = 1,
                FileName = "test.txt",
                FileExtension = ".txt",
                FilePath = @"C:\Temp\test.txt",
                FileSize = 1024,
                TaskId = 1,
            };

            var vm = CreateViewModel();

            await ((AsyncRelayCommandT<TaskAttachment>)vm.OpenAttachmentCommand).ExecuteAsync(
                attachment
            );

            _attachmentServiceMock.Verify(x => x.OpenFile(attachment), Times.Once);
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
}
