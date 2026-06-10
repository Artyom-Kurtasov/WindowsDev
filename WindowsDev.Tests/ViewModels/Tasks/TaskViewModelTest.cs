using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using Moq;
using WindowsDev.Business.Services.TaskService.Attachment.Interfaces;
using WindowsDev.Business.Services.TaskService.Comment.Interfaces;
using WindowsDev.Commands.NavigationManager.Interfaces;
using WindowsDev.Dialogs.Interfaces;
using WindowsDev.Domain.ProjectsModels;
using WindowsDev.Domain.TasksModels;
using WindowsDev.Domain.TasksModels.Enums;
using WindowsDev.Infrastructure;
using WindowsDev.ViewModels.Projects;
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

        public TaskViewModelTest()
        {
            _commentServiceMock = new Mock<ICommentService>();
            _dialogServiceMock = new Mock<IDialogService>();
            _navigationServiceMock = new Mock<INavigationService>();
            _attachmentServiceMock = new Mock<IAttacmentService>();
            _loggerMock = new Mock<ILogger<TaskViewModel>>();
            _dialogCoordinatorMock = new Mock<IDialogCoordinator>();
        }

        private TaskViewModel CreateViewModel()
        {
            return new TaskViewModel(
                _commentServiceMock.Object,
                _dialogServiceMock.Object,
                _navigationServiceMock.Object,
                _attachmentServiceMock.Object,
                _loggerMock.Object,
                _dialogCoordinatorMock.Object);
        }

        private ProjectsInfo CreateTestProject(int id = 1, string name = "Test Project")
        {
            return new ProjectsInfo
            {
                Id = id,
                Name = name,
                UserId = 1,
                CreatedAt = DateTime.Today
            };
        }

        private TasksInfo CreateTestTask(int id = 1, string name = "Test Task", int projectId = 1)
        {
            return new TasksInfo
            {
                Id = id,
                Name = name,
                Description = "Test Description",
                ProjectId = projectId,
                Priority = TaskPriority.Normal,
                Progress = 0,
                Status = TaskStatus.InProgress,
                DeadLine = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };
        }

        private List<Comments> CreateTestComments(int count = 2)
        {
            var comments = new List<Comments>();
            for (int i = 1; i <= count; i++)
            {
                comments.Add(new Comments
                {
                    Id = i,
                    Text = $"Comment {i}",
                    CreatedAt = DateTime.UtcNow,
                    Author = "author",
                    TaskId = i,
                });
            }
            return comments;
        }

        private List<TaskAttachment> CreateTestAttachments(int count = 2)
        {
            var attachments = new List<TaskAttachment>();
            for (int i = 1; i <= count; i++)
            {
                attachments.Add(new TaskAttachment
                {
                    Id = i,
                    FileName = $"file{i}.txt",
                    FilePath = $"C:\\temp\\file{i}.txt",
                    FileExtension = "txt",
                    FileSize = 1.0,
                    TaskId = i
                });
            }
            return attachments;
        }

        private void SetupDialogCoordinatorMock()
        {
            _dialogCoordinatorMock
                .Setup(x => x.ShowMessageAsync(
                    It.IsAny<TaskViewModel>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(MessageDialogResult.Affirmative);
        }

        [Fact]
        public async Task InitializationAsync_WhenTaskAndProjectProvided_LoadsTaskCommentsAndAttachments()
        {
            var vm = CreateViewModel();
            var project = CreateTestProject();
            var task = CreateTestTask();
            var comments = CreateTestComments();
            var attachments = CreateTestAttachments();

            _commentServiceMock
                .Setup(x => x.GetComments(task.Id))
                .ReturnsAsync(comments);

            _attachmentServiceMock
                .Setup(x => x.GetAttachmentsAsync(task.Id))
                .ReturnsAsync(attachments);

            await vm.InitializationAsync(task, project);

            Assert.Equal(task, vm.CurrentTask);
            Assert.Equal(project, vm.Project);
            Assert.Equal(2, vm.Comments?.Count);
            Assert.Equal(2, vm.Attachments?.Count);
        }

        [Fact]
        public async Task InitializationAsync_WhenNoTaskProvided_ThrowsArgumentNullException()
        {
            var vm = CreateViewModel();
            var project = CreateTestProject();

            await Assert.ThrowsAsync<ArgumentNullException>(
                () => vm.InitializationAsync(project));
        }

        [Fact]
        public async Task InitializationAsync_WhenNoProjectProvided_ThrowsArgumentNullException()
        {
            var vm = CreateViewModel();
            var task = CreateTestTask();

            await Assert.ThrowsAsync<ArgumentNullException>(
                () => vm.InitializationAsync(task));
        }

        [Fact]
        public async Task InitializationAsync_WhenGetCommentsThrowsException_ShowsErrorMessageAndSetsEmptyComments()
        {
            var vm = CreateViewModel();
            var project = CreateTestProject();
            var task = CreateTestTask();

            _commentServiceMock
                .Setup(x => x.GetComments(task.Id))
                .ThrowsAsync(new Exception());

            _attachmentServiceMock
                .Setup(x => x.GetAttachmentsAsync(task.Id))
                .ReturnsAsync(new List<TaskAttachment>());

            SetupDialogCoordinatorMock();

            await vm.InitializationAsync(task, project);

            Assert.Empty(vm.Comments);
            _dialogCoordinatorMock.Verify(x => x.ShowMessageAsync(
                It.IsAny<TaskViewModel>(),
                It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task InitializationAsync_WhenGetAttachmentsThrowsException_ShowsErrorMessageAndSetsEmptyAttachments()
        {
            var vm = CreateViewModel();
            var project = CreateTestProject();
            var task = CreateTestTask();
            var comments = CreateTestComments();

            _commentServiceMock
                .Setup(x => x.GetComments(task.Id))
                .ReturnsAsync(comments);

            _attachmentServiceMock
                .Setup(x => x.GetAttachmentsAsync(task.Id))
                .ThrowsAsync(new Exception());

            SetupDialogCoordinatorMock();

            await vm.InitializationAsync(task, project);

            Assert.Empty(vm.Attachments);
            _dialogCoordinatorMock.Verify(x => x.ShowMessageAsync(
                It.IsAny<TaskViewModel>(),
                It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task SwitchToProjectCommand_WhenExecuted_NavigatesToProjectViewModel()
        {
            var vm = CreateViewModel();
            var project = CreateTestProject();
            var task = CreateTestTask();
            await vm.InitializationAsync(task, project);

            ((RelayCommand)vm.SwitchToProjectCommand).Execute(null);

            _navigationServiceMock.Verify(x => x.NavigateTo<ProjectViewModel>(project), Times.Once);
        }

        [Fact]
        public void SwitchToProjectCommand_CanExecute_ReturnsTrue()
        {
            var vm = CreateViewModel();

            var result = ((RelayCommand)vm.SwitchToProjectCommand).CanExecute(null);

            Assert.True(result);
        }

        [Fact]
        public async Task EditTaskCommand_WhenExecuted_ShowsEditTaskDialog()
        {
            var vm = CreateViewModel();
            var project = CreateTestProject();
            var task = CreateTestTask();
            await vm.InitializationAsync(task, project);

            await ((AsyncRelayCommand)vm.EditTaskCommand).ExecuteAsync(null);

            _dialogServiceMock.Verify(x => x.ShowTaskDialogAsync<TaskDialogView, EditTaskViewModel>(
                vm, task), Times.Once);
        }

        [Fact]
        public async Task AddCommentCommand_WhenNewCommentIsValid_AddsCommentAndClearsInput()
        {
            var vm = CreateViewModel();
            var project = CreateTestProject();
            var task = CreateTestTask();
            var comments = CreateTestComments();
            var attachments = CreateTestAttachments();
            var newComment = new Comments
            {
                Id = 3,
                Text = "New comment",
                CreatedAt = DateTime.UtcNow,
                Author = "author",
                TaskId = task.Id
            };

            _commentServiceMock
                .Setup(x => x.GetComments(task.Id))
                .ReturnsAsync(comments);

            _attachmentServiceMock
                .Setup(x => x.GetAttachmentsAsync(task.Id))
                .ReturnsAsync(attachments);

            await vm.InitializationAsync(task, project);
            vm.NewComment = "New comment";

            _commentServiceMock
                .Setup(x => x.AddComment(task.Id, "New comment"))
                .ReturnsAsync(newComment);

            await ((AsyncRelayCommand)vm.AddCommentCommand).ExecuteAsync(null);

            Assert.Contains(newComment, vm.Comments);
            Assert.Equal(string.Empty, vm.NewComment);
            _commentServiceMock.Verify(x => x.AddComment(task.Id, "New comment"), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task AddCommentCommand_WhenNewCommentIsNullOrEmpty_DoesNotAddComment(string? newComment)
        {
            var vm = CreateViewModel();
            var project = CreateTestProject();
            var task = CreateTestTask();
            await vm.InitializationAsync(task, project);
            vm.NewComment = newComment;

            await ((AsyncRelayCommand)vm.AddCommentCommand).ExecuteAsync(null);

            _commentServiceMock.Verify(x => x.AddComment(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AddCommentCommand_WhenExceptionOccurs_ShowsErrorMessage()
        {
            var vm = CreateViewModel();
            var project = CreateTestProject();
            var task = CreateTestTask();
            var comments = CreateTestComments();
            var attachments = CreateTestAttachments();

            _commentServiceMock
                .Setup(x => x.GetComments(task.Id))
                .ReturnsAsync(comments);

            _attachmentServiceMock
                .Setup(x => x.GetAttachmentsAsync(task.Id))
                .ReturnsAsync(attachments);

            await vm.InitializationAsync(task, project);
            vm.NewComment = "New comment";

            _commentServiceMock
                .Setup(x => x.AddComment(It.IsAny<int>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception());

            SetupDialogCoordinatorMock();

            await ((AsyncRelayCommand)vm.AddCommentCommand).ExecuteAsync(null);

            _dialogCoordinatorMock.Verify(x => x.ShowMessageAsync(
                It.IsAny<TaskViewModel>(),
                "Error",
                "Failed to add comment. Try again",
                MessageDialogStyle.Affirmative,
                It.IsAny<MetroDialogSettings>()), Times.Once);
        }

        [Fact]
        public void RefreshTask_UpdatesAllProperties()
        {
            var vm = CreateViewModel();
            var project = CreateTestProject();
            var task = CreateTestTask();
            vm.InitializationAsync(task, project).Wait();

            var propertiesChanged = new List<string>();
            vm.PropertyChanged += (s, e) => propertiesChanged.Add(e.PropertyName);

            vm.RefreshTask();

            Assert.Contains(nameof(vm.Name), propertiesChanged);
            Assert.Contains(nameof(vm.Description), propertiesChanged);
            Assert.Contains(nameof(vm.Priority), propertiesChanged);
            Assert.Contains(nameof(vm.Progress), propertiesChanged);
            Assert.Contains(nameof(vm.Status), propertiesChanged);
            Assert.Contains(nameof(vm.DeadLine), propertiesChanged);
        }
    }
}