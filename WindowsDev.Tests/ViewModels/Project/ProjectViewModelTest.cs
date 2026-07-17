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
            _dialogServiceMock = new();
            _navigationServiceMock = new();
            _taskServiceMock = new();
            _dialogCoordinatorMock = new();
            _loggerMock = new();
            _languageChangerMock = new();

            _languageChangerMock
                .Setup(x => x.Translate(It.IsAny<string>()))
                .Returns((string key) => key);
        }

        private ProjectViewModel CreateViewModel(ProjectsInfo? project = null)
        {
            return new ProjectViewModel(
                project ?? CreateProject(),
                _dialogCoordinatorMock.Object,
                _navigationServiceMock.Object,
                _taskServiceMock.Object,
                _dialogServiceMock.Object,
                _loggerMock.Object,
                _languageChangerMock.Object
            );
        }

        private ProjectsInfo CreateProject()
        {
            return new()
            {
                Id = 1,
                Name = "Test Project",
                Description = "Description",
                UserId = 1,
                CreatedAt = DateTime.Today,
            };
        }

        private TasksInfo CreateTask(int id)
        {
            return new()
            {
                Id = id,
                Name = $"Task {id}",
                ProjectId = 1,
                Status = TaskStatus.InProgress,
                Priority = TaskPriority.Normal,
                CreatedAt = DateTime.Today,
                DeadLine = DateTime.Today.AddDays(7),
                Progress = 0,
            };
        }

        private List<TasksInfo> CreateTasks()
        {
            return new() { CreateTask(1), CreateTask(2), CreateTask(3) };
        }

        private void SetupTasks()
        {
            var tasks = CreateTasks();

            _taskServiceMock.Setup(x => x.GetTasksCountAsync(1)).ReturnsAsync(tasks.Count);

            _taskServiceMock
                .Setup(x => x.GetTasksAsync(It.IsAny<TaskFilter>()))
                .ReturnsAsync(Result<List<TasksInfo>>.Success(tasks));
        }

        private void SetupErrorDialog()
        {
            _dialogCoordinatorMock
                .Setup(x =>
                    x.ShowMessageAsync(
                        It.IsAny<ProjectViewModel>(),
                        DialogTitles.Error,
                        CommonErrors.UnexpectedError,
                        MessageDialogStyle.Affirmative
                    )
                )
                .ReturnsAsync(MessageDialogResult.Affirmative);
        }

        [Fact]
        public void Constructor_WhenCalled_SetsProject()
        {
            var project = CreateProject();

            var vm = CreateViewModel(project);

            Assert.Equal(project, vm.CurrentProject);
            Assert.Equal(project.Name, vm.Name);
            Assert.Equal(project.Description, vm.Description);
        }

        [Fact]
        public async Task Constructor_WhenCalled_LoadsTasks()
        {
            SetupTasks();

            var vm = CreateViewModel();

            Assert.Equal(3, vm.Tasks.Count);
            Assert.Equal(1, vm.CurrentPage);
            Assert.Equal(1, vm.TotalCountOfPages);

            _taskServiceMock.Verify(x => x.GetTasksCountAsync(1), Times.Once);
            _taskServiceMock.Verify(x => x.GetTasksAsync(It.IsAny<TaskFilter>()), Times.Once);
        }

        [Fact]
        public async Task Constructor_WhenLoadingFails_ShowsError()
        {
            SetupErrorDialog();

            _taskServiceMock.Setup(x => x.GetTasksCountAsync(1)).ThrowsAsync(new Exception());

            var vm = CreateViewModel();

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
        public async Task RefreshAsync_WhenCalled_LoadsTasks()
        {
            SetupTasks();

            var vm = CreateViewModel();

            _taskServiceMock.Invocations.Clear();

            await vm.RefreshAsync();

            _taskServiceMock.Verify(x => x.GetTasksCountAsync(1), Times.Once);
            _taskServiceMock.Verify(x => x.GetTasksAsync(It.IsAny<TaskFilter>()), Times.Once);
        }

        [Fact]
        public void SwitchToMainViewCommand_WhenExecuted_NavigatesToMainAndDisposes()
        {
            var vm = CreateViewModel();

            ((RelayCommand)vm.SwitchToMainViewCommand).Execute(null);

            _navigationServiceMock.Verify(x => x.NavigateTo<MainWindowViewModel>(), Times.Once);

            Assert.Null(vm.CurrentProject);

            Assert.Null(vm.Tasks);
        }

        [Fact]
        public async Task OpenDialogCommand_WhenExecuted_ShowsTaskDialog()
        {
            var vm = CreateViewModel();

            await ((AsyncRelayCommand)vm.OpenDialogCommand).ExecuteAsync(null);

            _dialogServiceMock.Verify(
                x => x.ShowDialogAsync<TaskDialogView, CreateTaskViewModel>(vm, 1),
                Times.Once
            );
        }

        [Fact]
        public async Task OpenTaskCommand_WhenExecuted_NavigatesToTask()
        {
            var vm = CreateViewModel();

            var task = CreateTask(1);

            await ((AsyncRelayCommandT<TasksInfo>)vm.OpenTaskCommand).ExecuteAsync(task);

            _navigationServiceMock.Verify(
                x => x.NavigateTo<TaskViewModel>(vm.CurrentProject, task),
                Times.Once
            );
        }

        [Fact]
        public async Task DeleteSelectedTasksCommand_WhenTasksSelected_DeletesAndRemovesThem()
        {
            SetupTasks();

            var vm = CreateViewModel();

            vm.Tasks[0].IsSelected = true;
            vm.Tasks[1].IsSelected = true;

            await ((AsyncRelayCommand)vm.DeleteSelectedTasksCommand).ExecuteAsync(null);

            Assert.Single(vm.Tasks);
            Assert.Contains(vm.Tasks, x => x.Id == 3);
            Assert.DoesNotContain(vm.Tasks, x => x.Id == 1);
            Assert.DoesNotContain(vm.Tasks, x => x.Id == 2);

            _taskServiceMock.Verify(x => x.DeleteAsync(1), Times.Once);
            _taskServiceMock.Verify(x => x.DeleteAsync(2), Times.Once);
        }

        [Fact]
        public async Task DeleteSelectedTasksCommand_WhenNothingSelected_KeepsCollection()
        {
            SetupTasks();

            var vm = CreateViewModel();

            await ((AsyncRelayCommand)vm.DeleteSelectedTasksCommand).ExecuteAsync(null);

            Assert.Equal(3, vm.Tasks.Count);

            _taskServiceMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task DeleteSelectedTasksCommand_WhenException_ShowsErrorAndKeepsTask()
        {
            SetupErrorDialog();

            SetupTasks();

            var vm = CreateViewModel();

            vm.Tasks[0].IsSelected = true;

            _taskServiceMock.Setup(x => x.DeleteAsync(1)).ThrowsAsync(new Exception());

            await ((AsyncRelayCommand)vm.DeleteSelectedTasksCommand).ExecuteAsync(null);

            Assert.Equal(3, vm.Tasks.Count);
            Assert.Contains(vm.Tasks, x => x.Id == 1);

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
        public async Task NextPageCommand_WhenCanGoNext_LoadsNextPage()
        {
            _taskServiceMock.Setup(x => x.GetTasksCountAsync(1)).ReturnsAsync(30);

            _taskServiceMock
                .Setup(x => x.GetTasksAsync(It.Is<TaskFilter>(x => x.Page == 2)))
                .ReturnsAsync(
                    Result<List<TasksInfo>>.Success(new() { CreateTask(4), CreateTask(5) })
                );

            var vm = CreateViewModel();

            await ((AsyncRelayCommand)vm.NextPageCommand).ExecuteAsync(null);

            Assert.Equal(2, vm.CurrentPage);
            Assert.Equal(2, vm.TotalCountOfPages);
            Assert.Equal(2, vm.Tasks.Count);
        }

        [Fact]
        public async Task NextPageCommand_WhenLastPage_DoesNothing()
        {
            _taskServiceMock.Setup(x => x.GetTasksCountAsync(1)).ReturnsAsync(10);

            var vm = CreateViewModel();

            _taskServiceMock.Invocations.Clear();

            await ((AsyncRelayCommand)vm.NextPageCommand).ExecuteAsync(null);

            Assert.Equal(1, vm.CurrentPage);

            _taskServiceMock.Verify(x => x.GetTasksAsync(It.IsAny<TaskFilter>()), Times.Never);
        }

        [Fact]
        public async Task SearchFilter_WhenChanged_LoadsTasksWithFilter()
        {
            SetupTasks();

            var vm = CreateViewModel();

            _taskServiceMock.Invocations.Clear();

            vm.SearchFilter = "test";

            await Task.Delay(50);

            _taskServiceMock.Verify(
                x => x.GetTasksAsync(It.Is<TaskFilter>(f => f.Seacrh == "test")),
                Times.Once
            );
        }

        [Fact]
        public void Dispose_WhenCalled_ClearsData()
        {
            var vm = CreateViewModel();

            vm.Dispose();

            Assert.Null(vm.CurrentProject);

            Assert.Null(vm.Tasks);
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
