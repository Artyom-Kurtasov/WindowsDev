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
                .Returns<string>(x => x);
        }



        private TaskViewModel CreateViewModel(
            ProjectsInfo? project = null,
            TasksInfo? task = null)
        {
            project ??= CreateTestProject();
            task ??= CreateTestTask();


            return new TaskViewModel(
                project,
                task,
                _commentServiceMock.Object,
                _dialogServiceMock.Object,
                _navigationServiceMock.Object,
                _attachmentServiceMock.Object,
                _loggerMock.Object,
                _dialogCoordinatorMock.Object,
                _languageChangerMock.Object);
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



        private ProjectsInfo CreateTestProject()
        {
            return new ProjectsInfo
            {
                Id = 1,
                Name = "Test Project",
                UserId = 1,
                CreatedAt = DateTime.UtcNow
            };
        }



        private TasksInfo CreateTestTask()
        {
            return new TasksInfo
            {
                Id = 1,
                Name = "Test Task",
                Description = "Description",
                ProjectId = 1,
                Priority = TaskPriority.Normal,
                Progress = 0,
                Status = TaskStatus.InProgress,
                DeadLine = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };
        }



        private void SetupDialogCoordinatorMock()
        {
            _dialogCoordinatorMock
                .Setup(x => x.ShowMessageAsync(
                    It.IsAny<TaskViewModel>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    MessageDialogStyle.Affirmative,
                    It.IsAny<MetroDialogSettings>()))
                .ReturnsAsync(MessageDialogResult.Affirmative);
        }



        [Fact]
        public void Constructor_WhenTaskAndProjectProvided_SetsData()
        {
            var project = CreateTestProject();
            var task = CreateTestTask();

            SetupSuccessfulLoading(task.Id);

            var vm = CreateViewModel(project, task);


            Assert.Equal(task, vm.CurrentTask);
            Assert.Equal(project, vm.Project);
            Assert.Equal(task.Name, vm.Name);
        }



        [Fact]
        public async Task Constructor_WhenCommentsLoadFails_ShowsError()
        {
            var project = CreateTestProject();
            var task = CreateTestTask();


            _commentServiceMock
                .Setup(x => x.GetComments(task.Id))
                .ThrowsAsync(new Exception());


            SetupDialogCoordinatorMock();


            var vm = CreateViewModel(project, task);


            await Task.Delay(200);


            Assert.Empty(vm.Comments);


            _dialogCoordinatorMock.Verify(x =>
                x.ShowMessageAsync(
                    It.IsAny<TaskViewModel>(),
                    DialogTitles.Error,
                    TaskErrors.LoadCommentsFailed,
                    MessageDialogStyle.Affirmative,
                    It.IsAny<MetroDialogSettings>()),
                Times.Once);
        }



        [Fact]
        public async Task Constructor_WhenAttachmentsLoadFails_ShowsError()
        {
            var project = CreateTestProject();
            var task = CreateTestTask();


            _attachmentServiceMock
                .Setup(x => x.GetAttachmentsAsync(task.Id))
                .ThrowsAsync(new Exception());


            SetupDialogCoordinatorMock();


            var vm = CreateViewModel(project, task);


            await Task.Delay(200);


            Assert.Empty(vm.Attachments);


            _dialogCoordinatorMock.Verify(x =>
                x.ShowMessageAsync(
                    It.IsAny<TaskViewModel>(),
                    DialogTitles.Error,
                    TaskErrors.LoadAttachmentsFailed,
                    MessageDialogStyle.Affirmative,
                    It.IsAny<MetroDialogSettings>()),
                Times.Once);
        }



        [Fact]
        public async Task SwitchToProjectCommand_Navigates()
        {
            var project = CreateTestProject();

            SetupSuccessfulLoading(1);

            var vm = CreateViewModel(project);


            await Task.Delay(100);


            ((RelayCommand)vm.SwitchToProjectCommand)
                .Execute(null);



            _navigationServiceMock.Verify(x =>
                x.NavigateTo<ProjectViewModel>(project),
                Times.Once);
        }



        [Fact]
        public async Task EditTaskCommand_ShowsDialog()
        {
            var task = CreateTestTask();

            SetupSuccessfulLoading(task.Id);


            var vm = CreateViewModel(task: task);


            await Task.Delay(100);


            await ((AsyncRelayCommand)vm.EditTaskCommand)
                .ExecuteAsync(null);



            _dialogServiceMock.Verify(x =>
                x.ShowDialogAsync<TaskDialogView, EditTaskViewModel>(
                    vm,
                    task),
                Times.Once);
        }



        [Fact]
        public async Task AddComment_WhenValid_AddsComment()
        {
            var task = CreateTestTask();


            var comment = new Comments
            {
                Id = 1,
                Text = "New",
                TaskId = task.Id,
                CreatedAt = DateTime.UtcNow,
                Author = "Test User"
            };


            SetupSuccessfulLoading(task.Id);


            _commentServiceMock
                .Setup(x => x.AddComment(task.Id, "New"))
                .ReturnsAsync(Result<Comments>.Success(comment));


            var vm = CreateViewModel(task: task);


            await Task.Delay(100);


            vm.NewComment = "New";


            await ((AsyncRelayCommand)vm.AddCommentCommand)
                .ExecuteAsync(null);



            Assert.Contains(comment, vm.Comments);
            Assert.Equal(string.Empty, vm.NewComment);
        }



        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task AddComment_WhenEmpty_DoesNothing(string? text)
        {
            var task = CreateTestTask();

            SetupSuccessfulLoading(task.Id);


            var vm = CreateViewModel(task: task);


            await Task.Delay(100);


            vm.NewComment = text;


            await ((AsyncRelayCommand)vm.AddCommentCommand)
                .ExecuteAsync(null);



            _commentServiceMock.Verify(x =>
                x.AddComment(
                    It.IsAny<int>(),
                    It.IsAny<string>()),
                Times.Never);
        }



        [Fact]
        public async Task AddComment_WhenException_ShowsError()
        {
            var task = CreateTestTask();


            SetupSuccessfulLoading(task.Id);


            _commentServiceMock
                .Setup(x => x.AddComment(
                    It.IsAny<int>(),
                    It.IsAny<string>()))
                .ThrowsAsync(new Exception());


            SetupDialogCoordinatorMock();


            var vm = CreateViewModel(task: task);


            await Task.Delay(100);


            vm.NewComment = "Test";


            await ((AsyncRelayCommand)vm.AddCommentCommand)
                .ExecuteAsync(null);



            _dialogCoordinatorMock.Verify(x =>
                x.ShowMessageAsync(
                    It.IsAny<TaskViewModel>(),
                    DialogTitles.Error,
                    CommonErrors.UnexpectedError,
                    MessageDialogStyle.Affirmative,
                    It.IsAny<MetroDialogSettings>()),
                Times.Once);
        }



        [Fact]
        public void RefreshTask_RaisesPropertiesChanged()
        {
            var task = CreateTestTask();

            SetupSuccessfulLoading(task.Id);


            var vm = CreateViewModel(task: task);


            var changed = new List<string>();


            vm.PropertyChanged += (_, e) =>
                changed.Add(e.PropertyName);



            vm.RefreshTask();



            Assert.Contains(nameof(vm.Name), changed);
            Assert.Contains(nameof(vm.Description), changed);
            Assert.Contains(nameof(vm.Priority), changed);
            Assert.Contains(nameof(vm.Progress), changed);
            Assert.Contains(nameof(vm.Status), changed);
            Assert.Contains(nameof(vm.DeadLine), changed);
        }
    }
}