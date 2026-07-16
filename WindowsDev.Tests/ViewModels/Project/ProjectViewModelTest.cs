using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using Moq;
using WindowsDev.Business.Primitives;
using WindowsDev.Business.Services.Localization.Interfaces;
using WindowsDev.Business.Services.TaskService;
using WindowsDev.Business.Services.TaskService.Interfaces;
using WindowsDev.Dialogs.Interfaces;
using WindowsDev.Domain;
using WindowsDev.Domain.DialogsMessages.Errors;
using WindowsDev.Domain.ProjectsModels;
using WindowsDev.Domain.TasksModels;
using WindowsDev.Domain.TasksModels.Enums;
using WindowsDev.Infrastructure;
using WindowsDev.NavigationManager.Interfaces;
using WindowsDev.ViewModels.Main;
using WindowsDev.ViewModels.Projects;
using WindowsDev.ViewModels.Tasks;
using WindowsDev.ViewModels.Tasks.Dialog;
using WindowsDev.Views.Tasks;
using TaskStatus = WindowsDev.Domain.TasksModels.Enums.TaskStatus;

namespace WindowsDev.Tests.ViewModels.Projects
{
    public class ProjectViewModelTest
    {
        private readonly Mock<IDialogService> _dialogServiceMock;
        private readonly Mock<INavigationService> _navigationServiceMock;
        private readonly Mock<ITaskService> _taskServiceMock;
        private readonly Mock<IDialogCoordinator> _dialogCoordinatorMock;
        private readonly Mock<ILogger<ProjectViewModel>> _loggerMock;
        private readonly Mock<ILanguageChanger> _languageChangerMock;


        public ProjectViewModelTest()
        {
            _dialogServiceMock = new Mock<IDialogService>();
            _navigationServiceMock = new Mock<INavigationService>();
            _taskServiceMock = new Mock<ITaskService>();
            _dialogCoordinatorMock = new Mock<IDialogCoordinator>();
            _loggerMock = new Mock<ILogger<ProjectViewModel>>();
            _languageChangerMock = new Mock<ILanguageChanger>();


            _languageChangerMock
                .Setup(x => x.Translate(It.IsAny<string>()))
                .Returns((string key) => key);
        }


        private ProjectViewModel CreateViewModel(
            ProjectsInfo? project = null)
        {
            return new ProjectViewModel(
                project ?? CreateProject(),
                _dialogCoordinatorMock.Object,
                _navigationServiceMock.Object,
                _taskServiceMock.Object,
                _dialogServiceMock.Object,
                _loggerMock.Object,
                _languageChangerMock.Object);
        }


        private ProjectsInfo CreateProject()
        {
            return new ProjectsInfo
            {
                Id = 1,
                Name = "Test Project",
                Description = "Description",
                UserId = 1,
                CreatedAt = DateTime.Today
            };
        }


        private List<TasksInfo> CreateTasks()
        {
            return new List<TasksInfo>
            {
                new()
                {
                    Id = 1,
                    Name = "Task 1",
                    ProjectId = 1,
                    Status = TaskStatus.InProgress,
                    Priority = TaskPriority.Normal,
                    CreatedAt= DateTime.Today,
                    DeadLine = DateTime.Today.AddDays(7),
                    Progress = 0,
                }
            };
        }


        private void SetupTasks()
        {
            var tasks = CreateTasks();

            _taskServiceMock
                .Setup(x => x.GetTasksCountAsync(1))
                .ReturnsAsync(tasks.Count);


            _taskServiceMock
                .Setup(x => x.GetTasksAsync(It.IsAny<TaskFilter>()))
                .ReturnsAsync(Result<List<TasksInfo>>.Success(tasks));
        }


        private void SetupErrorDialog()
        {
            _dialogCoordinatorMock
                .Setup(x => x.ShowMessageAsync(
                    It.IsAny<ProjectViewModel>(),
                    DialogTitles.Error,
                    CommonErrors.UnexpectedError,
                    MessageDialogStyle.Affirmative,
                    It.IsAny<MetroDialogSettings>()))
                .ReturnsAsync(MessageDialogResult.Affirmative);
        }


        [Fact]
        public async Task Constructor_LoadsTasks()
        {
            SetupTasks();

            var vm = CreateViewModel();

            await Task.Delay(50);

            Assert.Single(vm.Tasks);

            _taskServiceMock.Verify(
                x => x.GetTasksCountAsync(1),
                Times.Once);
        }


        [Fact]
        public void Constructor_SetsProject()
        {
            var project = CreateProject();

            var vm = CreateViewModel(project);

            Assert.Equal(project, vm.CurrentProject);
            Assert.Equal(project.Name, vm.Name);
        }


        [Fact]
        public void SwitchToMainView_Navigates()
        {
            var vm = CreateViewModel();

            ((RelayCommand)vm.SwitchToMainViewCommand)
                .Execute(null);

            _navigationServiceMock.Verify(
                x => x.NavigateTo<MainWindowViewModel>(),
                Times.Once);
        }


        [Fact]
        public async Task OpenDialog_ShowsTaskDialog()
        {
            var vm = CreateViewModel();

            await ((AsyncRelayCommand)vm.OpenDialogCommand)
                .ExecuteAsync(null);


            _dialogServiceMock.Verify(
                x => x.ShowDialogAsync<
                    TaskDialogView,
                    CreateTaskViewModel>(
                    vm,
                    1),
                Times.Once);
        }


        [Fact]
        public async Task OpenTask_NavigatesToTaskView()
        {
            var vm = CreateViewModel();

            var task = new TasksInfo
            {
                Id = 1,
                Name = "Task",
                Progress = 100,
                ProjectId = 1,
                Status = TaskStatus.Done,
                Priority = TaskPriority.Normal,
                DeadLine = DateTime.Today,
                CreatedAt = DateTime.Today
            };


            await ((AsyncRelayCommandT<TasksInfo>)
                vm.OpenTaskCommand)
                .ExecuteAsync(task);


            _navigationServiceMock.Verify(
                x => x.NavigateTo<TaskViewModel>(
                    vm.CurrentProject,
                    task),
                Times.Once);
        }

        [Fact]
        public async Task DeleteSelectedTasks_DeletesSelected()
        {
            SetupTasks();

            var vm = CreateViewModel();

            await Task.Delay(50);

            vm.Tasks[0].IsSelected = true;


            _taskServiceMock
                .Setup(x => x.DeleteAsync(1))
                .Returns(Task.CompletedTask);


            await ((AsyncRelayCommand)
                vm.DeleteSelectedTasksCommand)
                .ExecuteAsync(null);


            _taskServiceMock.Verify(
                x => x.DeleteAsync(1),
                Times.Once);
        }

        [Fact]
        public async Task DeleteSelectedTasks_WhenException_ShowsError()
        {
            SetupTasks();
            SetupErrorDialog();

            var vm = CreateViewModel();

            await Task.Delay(50);

            vm.Tasks[0].IsSelected = true;


            _taskServiceMock
                .Setup(x => x.DeleteAsync(1))
                .ThrowsAsync(new Exception());


            await ((AsyncRelayCommand)
                vm.DeleteSelectedTasksCommand)
                .ExecuteAsync(null);


            _dialogCoordinatorMock.Verify(
                x => x.ShowMessageAsync(
                    vm,
                    DialogTitles.Error,
                    CommonErrors.UnexpectedError,
                    MessageDialogStyle.Affirmative,
                    It.IsAny<MetroDialogSettings>()),
                Times.Once);
        }

        [Fact]
        public async Task GetTasks_WhenServiceFails_ShowsError()
        {
            SetupErrorDialog();


            _taskServiceMock
                .Setup(x => x.GetTasksCountAsync(1))
                .ReturnsAsync(1);


            _taskServiceMock
                .Setup(x => x.GetTasksAsync(
                    It.IsAny<TaskFilter>()))
                .ReturnsAsync(
                    Result<List<TasksInfo>>
                    .Failure("error"));


            var vm = CreateViewModel();

            await Task.Delay(50);


            _dialogCoordinatorMock.Verify(
                x => x.ShowMessageAsync(
                    vm,
                    DialogTitles.Error,
                    CommonErrors.UnexpectedError,
                    MessageDialogStyle.Affirmative,
                    It.IsAny<MetroDialogSettings>()),
                Times.Once);
        }

        [Fact]
        public async Task RefreshAsync_LoadsTasks()
        {
            SetupTasks();

            var vm = CreateViewModel();

            await Task.Delay(50);

            _taskServiceMock.Invocations.Clear();


            await vm.RefreshAsync();


            _taskServiceMock.Verify(
                x => x.GetTasksCountAsync(1),
                Times.Once);
        }
    }
}