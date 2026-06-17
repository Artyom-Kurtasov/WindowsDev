using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using Moq;
using WindowsDev.Business.Services.TaskService;
using WindowsDev.Business.Services.TaskService.Interfaces;
using WindowsDev.Dialogs.Interfaces;
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

        public ProjectViewModelTest()
        {
            _dialogServiceMock = new Mock<IDialogService>();
            _navigationServiceMock = new Mock<INavigationService>();
            _taskServiceMock = new Mock<ITaskService>();
            _dialogCoordinatorMock = new Mock<IDialogCoordinator>();
            _loggerMock = new Mock<ILogger<ProjectViewModel>>();
        }

        private ProjectViewModel CreateViewModel(ProjectsInfo project = null)
        {
            return new ProjectViewModel(
                project,
                _dialogCoordinatorMock.Object,
                _navigationServiceMock.Object,
                _taskServiceMock.Object,
                _dialogServiceMock.Object,
                _loggerMock.Object);
        }

        private ProjectsInfo CreateTestProject(int id = 1, string name = "Test Project", string description = "Test Description")
        {
            return new ProjectsInfo
            {
                Id = id,
                Name = name,
                Description = description,
                UserId = 1,
                CreatedAt = DateTime.Today
            };
        }

        private List<TasksInfo> CreateTestTasks(int count = 5, int projectId = 1)
        {
            var tasks = new List<TasksInfo>();
            int progress = 10;

            for (int i = 1; i <= count; i++)
            {
                tasks.Add(new TasksInfo
                {
                    Id = i,
                    Priority = TaskPriority.Normal,
                    Progress = progress,
                    CreatedAt = DateTime.Now,
                    DeadLine = DateTime.Now.AddDays(7),
                    Name = $"Task {i}",
                    ProjectId = projectId,
                    Status = TaskStatus.InProgress
                });
            }
            return tasks;
        }

        [Fact]
        public void Constructor_WhenProjectProvided_SetsProjectProperties()
        {
            var project = CreateTestProject();

            var vm = CreateViewModel(project);

            Assert.Equal(project, vm.CurrentProject);
            Assert.Equal(project.Name, vm.Name);
            Assert.Equal(project.Description, vm.Description);
        }

        [Fact]
        public async Task Constructor_LoadsTasks()
        {
            var project = CreateTestProject();
            var tasks = CreateTestTasks(5, project.Id);

            _taskServiceMock
                .Setup(x => x.GetTasksCountAsync(project.Id))
                .ReturnsAsync(5);

            _taskServiceMock
                .Setup(x => x.GetTasksAsync(It.IsAny<TaskFilter>()))
                .ReturnsAsync(tasks);

            var vm = new ProjectViewModel(
                project,
                _dialogCoordinatorMock.Object,
                _navigationServiceMock.Object,
                _taskServiceMock.Object,
                _dialogServiceMock.Object,
                _loggerMock.Object);

            Assert.Equal(5, vm.TaskItem.Count);
            Assert.Equal(1, vm.CurrentPage);
            Assert.Equal(1, vm.TotalCountOfPages);

            _taskServiceMock.Verify(x => x.GetTasksCountAsync(project.Id), Times.Once);
            _taskServiceMock.Verify(x => x.GetTasksAsync(It.Is<TaskFilter>(f =>
                f.ProjectId == project.Id &&
                f.Page == 1 &&
                f.PageSize == 15)), Times.Once);
        }

        [Fact]
        public void SwitchToMainViewCommand_WhenExecuted_NavigatesToMainWindow()
        {
            var vm = CreateViewModel();

            ((RelayCommand)vm.SwitchToMainViewCommand).Execute(null);

            _navigationServiceMock.Verify(x => x.NavigateTo<MainWindowViewModel>(), Times.Once);
        }

        [Fact]
        public void SwitchToMainViewCommand_CanExecute_ReturnsTrue()
        {
            var vm = CreateViewModel();

            var canExecute = ((RelayCommand)vm.SwitchToMainViewCommand).CanExecute(null);

            Assert.True(canExecute);
        }

        [Fact]
        public async Task OpenDialogCommand_WhenExecuted_ShowsTaskDialog()
        {
            var project = CreateTestProject();
            var vm = CreateViewModel(project);

            await ((AsyncRelayCommand)vm.OpenDialogCommand).ExecuteAsync(null);

            _dialogServiceMock.Verify(x => x.ShowDialogAsync<TaskDialogView, CreateTaskViewModel>(
                vm,
                project.Id), Times.Once);
        }

        [Fact]
        public async Task OpenTaskCommand_WhenExecuted_NavigatesToTaskViewModel()
        {
            var project = CreateTestProject();
            var vm = CreateViewModel(project);

            var task = new TasksInfo
            {
                Id = 1,
                Name = "Test Task",
                ProjectId = project.Id,
                Priority = TaskPriority.Normal,
                Progress = 10,
                Status = TaskStatus.Done,
                CreatedAt = DateTime.Now,
                DeadLine = DateTime.Now.AddDays(7)
            };

            await ((AsyncRelayCommandT<TasksInfo>)vm.OpenTaskCommand).ExecuteAsync(task);

            _navigationServiceMock.Verify(x => x.NavigateTo<TaskViewModel>(project, task), Times.Once);
        }

        [Fact]
        public async Task NextPageCommand_WhenCurrentPageLessThanTotalTasks_IncrementsPageAndLoadsTasks()
        {
            var project = CreateTestProject();
            var tasks = CreateTestTasks(5, project.Id);

            _taskServiceMock
                .Setup(x => x.GetTasksCountAsync(project.Id))
                .ReturnsAsync(30);

            _taskServiceMock
                .Setup(x => x.GetTasksAsync(It.IsAny<TaskFilter>()))
                .ReturnsAsync(tasks);

            var vm = CreateViewModel(project);
            vm.CurrentPage = 1;

            _taskServiceMock.Invocations.Clear();

            await ((AsyncRelayCommand)vm.NextPageCommand).ExecuteAsync(null);

            Assert.Equal(2, vm.CurrentPage);
            _taskServiceMock.Verify(x => x.GetTasksAsync(It.Is<TaskFilter>(f => f.Page == 2)), Times.Once);
        }

        [Fact]
        public async Task NextPageCommand_WhenCurrentPageEqualsTotalTasks_DoesNotLoadTasks()
        {
            var project = CreateTestProject();

            _taskServiceMock
                .Setup(x => x.GetTasksCountAsync(project.Id))
                .ReturnsAsync(15);

            var vm = CreateViewModel(project);
            vm.CurrentPage = 1;

            _taskServiceMock.Invocations.Clear();

            await ((AsyncRelayCommand)vm.NextPageCommand).ExecuteAsync(null);

            Assert.Equal(1, vm.CurrentPage);
            _taskServiceMock.Verify(x => x.GetTasksAsync(It.IsAny<TaskFilter>()), Times.Never);
        }

        [Fact]
        public async Task PrevPageCommand_WhenCurrentPageIsOne_DoesNotLoadTasks()
        {
            var project = CreateTestProject();

            _taskServiceMock
                .Setup(x => x.GetTasksCountAsync(project.Id))
                .ReturnsAsync(30);

            var vm = CreateViewModel(project);
            vm.CurrentPage = 1;

            _taskServiceMock.Invocations.Clear();

            await ((AsyncRelayCommand)vm.PrevPageCommand).ExecuteAsync(null);

            Assert.Equal(1, vm.CurrentPage);
            _taskServiceMock.Verify(x => x.GetTasksAsync(It.IsAny<TaskFilter>()), Times.Never);
        }

        [Fact]
        public async Task SearchFilter_WhenChanged_ReloadsTasksWithSearchText()
        {
            var project = CreateTestProject();
            var tasks = CreateTestTasks(5, project.Id);

            _taskServiceMock
                .Setup(x => x.GetTasksCountAsync(project.Id))
                .ReturnsAsync(5);

            _taskServiceMock
                .Setup(x => x.GetTasksAsync(It.IsAny<TaskFilter>()))
                .ReturnsAsync(tasks);

            var vm = CreateViewModel(project);

            vm.SearchFilter = "test";

            await Task.Delay(100);

            _taskServiceMock.Verify(x => x.GetTasksAsync(It.Is<TaskFilter>(f =>
                f.Seacrh == "test")), Times.AtLeastOnce);
        }

        [Theory]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, true)]
        [InlineData(false, true, true)]
        public async Task ShowFilters_WhenChanged_ReloadsTasksWithCorrectStatuses(
            bool showAll, bool showClosed, bool showInProgress)
        {
            var project = CreateTestProject();
            var tasks = CreateTestTasks(5, project.Id);

            _taskServiceMock
                .Setup(x => x.GetTasksCountAsync(project.Id))
                .ReturnsAsync(5);

            _taskServiceMock
                .Setup(x => x.GetTasksAsync(It.IsAny<TaskFilter>()))
                .ReturnsAsync(tasks);

            var vm = CreateViewModel(project);

            vm.ShowAll = showAll;
            vm.ShowClosed = showClosed;
            vm.ShowInProgress = showInProgress;

            await Task.Delay(100);

            _taskServiceMock.Verify(x => x.GetTasksAsync(It.IsAny<TaskFilter>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task DeleteSelectedTasksCommand_WhenTasksSelected_DeletesTasksAndRemovesFromCollection()
        {
            var project = CreateTestProject();
            var tasks = CreateTestTasks(3, project.Id);
            tasks[0].IsSelected = true;
            tasks[1].IsSelected = true;

            _taskServiceMock
                .Setup(x => x.GetTasksCountAsync(project.Id))
                .ReturnsAsync(3);

            _taskServiceMock
                .Setup(x => x.GetTasksAsync(It.IsAny<TaskFilter>()))
                .ReturnsAsync(tasks);

            _taskServiceMock
                .Setup(x => x.DeleteAsync(It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            var vm = CreateViewModel(project);

            await Task.Delay(100);

            _taskServiceMock.Invocations.Clear();

            await ((AsyncRelayCommand)vm.DeleteSelectedTasksCommand).ExecuteAsync(null);

            _taskServiceMock.Verify(x => x.DeleteAsync(tasks[0].Id), Times.Once);
            _taskServiceMock.Verify(x => x.DeleteAsync(tasks[1].Id), Times.Once);
            _taskServiceMock.Verify(x => x.DeleteAsync(tasks[2].Id), Times.Never);
        }

        [Fact]
        public async Task DeleteSelectedTasksCommand_WhenNoTasksSelected_DoesNotCallDelete()
        {
            var project = CreateTestProject();
            var tasks = CreateTestTasks(3, project.Id);

            _taskServiceMock
                .Setup(x => x.GetTasksCountAsync(project.Id))
                .ReturnsAsync(3);

            _taskServiceMock
                .Setup(x => x.GetTasksAsync(It.IsAny<TaskFilter>()))
                .ReturnsAsync(tasks);

            var vm = CreateViewModel(project);

            await Task.Delay(100);

            _taskServiceMock.Invocations.Clear();

            await ((AsyncRelayCommand)vm.DeleteSelectedTasksCommand).ExecuteAsync(null);

            _taskServiceMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetPageAsync_WhenExceptionOccurs_ShowsErrorMessage()
        {
            var project = CreateTestProject();

            _taskServiceMock
                .Setup(x => x.GetTasksCountAsync(project.Id))
                .ReturnsAsync(5);

            _taskServiceMock
                .Setup(x => x.GetTasksAsync(It.IsAny<TaskFilter>()))
                .ThrowsAsync(new Exception());

            _dialogCoordinatorMock
                .Setup(x => x.ShowMessageAsync(It.IsAny<ProjectViewModel>(),
                    It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(MessageDialogResult.Affirmative);

            var vm = CreateViewModel(project);

            await Task.Delay(100);

            _dialogCoordinatorMock.Verify(x => x.ShowMessageAsync(
                It.IsAny<ProjectViewModel>(),
                It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task DeleteSelectedTasksCommand_WhenExceptionOccurs_ShowsErrorMessage()
        {
            var project = CreateTestProject();
            var tasks = CreateTestTasks(1, project.Id);
            tasks[0].IsSelected = true;

            _taskServiceMock
                .Setup(x => x.GetTasksCountAsync(project.Id))
                .ReturnsAsync(1);

            _taskServiceMock
                .Setup(x => x.GetTasksAsync(It.IsAny<TaskFilter>()))
                .ReturnsAsync(tasks);

            _taskServiceMock
                .Setup(x => x.DeleteAsync(It.IsAny<int>()))
                .ThrowsAsync(new Exception());

            _dialogCoordinatorMock
                .Setup(x => x.ShowMessageAsync(It.IsAny<ProjectViewModel>(),
                    It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(MessageDialogResult.Affirmative);

            var vm = CreateViewModel(project);

            await Task.Delay(100);

            await ((AsyncRelayCommand)vm.DeleteSelectedTasksCommand).ExecuteAsync(null);

            _dialogCoordinatorMock.Verify(x => x.ShowMessageAsync(
                It.IsAny<ProjectViewModel>(),
                It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task RefreshAsync_ReloadsTasks()
        {
            var project = CreateTestProject();
            var tasks = CreateTestTasks(5, project.Id);

            _taskServiceMock
                .Setup(x => x.GetTasksCountAsync(project.Id))
                .ReturnsAsync(5);

            _taskServiceMock
                .Setup(x => x.GetTasksAsync(It.IsAny<TaskFilter>()))
                .ReturnsAsync(tasks);

            var vm = CreateViewModel(project);

            await Task.Delay(100);
            _taskServiceMock.Invocations.Clear();

            await vm.RefreshAsync();

            _taskServiceMock.Verify(x => x.GetTasksCountAsync(project.Id), Times.Once);
            _taskServiceMock.Verify(x => x.GetTasksAsync(It.IsAny<TaskFilter>()), Times.Once);
        }
    }
}