using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using Moq;
using WindowsDev.ApiClient.TaskClient;
using WindowsDev.Application.Common.Localization;
using WindowsDev.Application.Primitives;
using WindowsDev.Command;
using WindowsDev.Contracts.UI.Projects;
using WindowsDev.Contracts.UI.Tasks;
using WindowsDev.Domain.Enums;
using WindowsDev.Domain.Messages;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;
using WindowsDev.Services.Dialogs;
using WindowsDev.Services.Navigation;
using WindowsDev.ViewModels.Main;
using WindowsDev.ViewModels.Project;
using WindowsDev.ViewModels.Tasks;
using WindowsDev.ViewModels.Tasks.Dialogs;
using WindowsDev.Views.Tasks;
using TaskStatus = WindowsDev.Domain.Enums.TaskStatus;

namespace WindowsDev.Tests.ViewModels.Projects;

public class ProjectViewModelTest
{
    private readonly Mock<IDialogService> _dialogServiceMock;
    private readonly Mock<INavigationService> _navigationServiceMock;
    private readonly Mock<ITaskApiClient> _apiClientMock;
    private readonly Mock<ILogger<ProjectViewModel>> _loggerMock;
    private readonly Mock<ILanguageChanger> _languageChangerMock;

    public ProjectViewModelTest()
    {
        _dialogServiceMock = new();
        _navigationServiceMock = new();
        _apiClientMock = new();
        _loggerMock = new();
        _languageChangerMock = new();

        _languageChangerMock
            .Setup(x => x.Translate(It.IsAny<string>()))
            .Returns((string key) => key);

        _apiClientMock
            .Setup(x => x.GetTasksCountAsync(It.IsAny<int>()))
            .ReturnsAsync(Result<int>.Success(0));
        _apiClientMock
            .Setup(x =>
                x.GetTasksAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<List<TaskStatus>>()
                )
            )
            .ReturnsAsync(Result<List<TaskInfo>>.Success(new List<TaskInfo>()));
    }

    private ProjectViewModel CreateViewModel(ProjectInfo? project = null)
    {
        return new ProjectViewModel(
            project ?? CreateProject(),
            _navigationServiceMock.Object,
            _dialogServiceMock.Object,
            _apiClientMock.Object,
            _loggerMock.Object,
            _languageChangerMock.Object
        );
    }

    private ProjectInfo CreateProject()
    {
        return new ProjectInfo(
            1,
            "Test Project",
            "Description",
            DateTime.Today
        );
    }

    private TaskInfo CreateTask(int id)
    {
        return new TaskInfo
        {
            Id = id,
            Name = $"Task {id}",
            Description = "Description",
            Priority = TaskPriority.Medium,
            Progress = 0,
            Status = TaskStatus.InProgress,
            ProjectId = 1,
            CreatedAt = DateTime.Today,
            Deadline = DateTime.Today.AddDays(7)
        };
    }

    private List<TaskInfo> CreateTasks()
    {
        return new() { CreateTask(1), CreateTask(2), CreateTask(3) };
    }

    private void SetupTasks()
    {
        var tasks = CreateTasks();

        _apiClientMock
            .Setup(x => x.GetTasksCountAsync(1))
            .ReturnsAsync(Result<int>.Success(tasks.Count));

        _apiClientMock
            .Setup(x =>
                x.GetTasksAsync(
                    1,
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<List<TaskStatus>>()
                )
            )
            .ReturnsAsync(Result<List<TaskInfo>>.Success(tasks));
    }

    private void SetupErrorDialog()
    {
        _dialogServiceMock.Setup(x =>
            x.ShowMessageAsync(
                DialogTitles.Error,
                CommonErrors.UnexpectedError,
                MessageDialogStyle.Affirmative
            )
        );
    }

    private static async Task WaitUntilAsync(Func<bool> condition, int timeoutMs = 2000)
    {
        var start = Environment.TickCount;
        while (!condition())
        {
            if (Environment.TickCount - start > timeoutMs)
                throw new TimeoutException("Condition was not met in time.");

            await Task.Delay(10);
        }
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

        await WaitUntilAsync(() => vm.Tasks.Count == 3);

        Assert.Equal(1, vm.CurrentPage);
        Assert.Equal(1, vm.TotalCountOfPages);

        _apiClientMock.Verify(x => x.GetTasksCountAsync(1), Times.Once);
        _apiClientMock.Verify(
            x =>
                x.GetTasksAsync(
                    1,
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<List<TaskStatus>>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Constructor_WhenLoadingFails_ShowsError()
    {
        SetupErrorDialog();

        _apiClientMock
            .Setup(x => x.GetTasksCountAsync(1))
            .ThrowsAsync(new Exception());

        var vm = CreateViewModel();

        await WaitUntilAsync(() => true); // дать возможность отработать fire-and-forget
        await Task.Delay(50);

        _dialogServiceMock.Verify(
            x =>
                x.ShowMessageAsync(
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
        await WaitUntilAsync(() => vm.Tasks.Count == 3);

        _apiClientMock.Invocations.Clear();

        await vm.RefreshAsync();

        _apiClientMock.Verify(x => x.GetTasksCountAsync(1), Times.Once);
        _apiClientMock.Verify(
            x =>
                x.GetTasksAsync(
                    1,
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<List<TaskStatus>>()
                ),
            Times.Once
        );
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
            x => x.ShowDialogAsync<TaskDialogView, CreateTaskViewModel>(1),
            Times.Once
        );
    }

    [Fact]
    public async Task OpenTaskCommand_WhenExecuted_NavigatesToTask()
    {
        var vm = CreateViewModel();

        var task = CreateTask(1);

        await ((AsyncRelayCommandT<TaskInfo>)vm.OpenTaskCommand).ExecuteAsync(task);

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
        await WaitUntilAsync(() => vm.Tasks.Count == 3);

        vm.Tasks[0].IsSelected = true;
        vm.Tasks[1].IsSelected = true;

        await ((AsyncRelayCommand)vm.DeleteSelectedTasksCommand).ExecuteAsync(null);

        Assert.Single(vm.Tasks);
        Assert.Contains(vm.Tasks, x => x.Task.Id == 3);
        Assert.DoesNotContain(vm.Tasks, x => x.Task.Id == 1);
        Assert.DoesNotContain(vm.Tasks, x => x.Task.Id == 2);

        _apiClientMock.Verify(x => x.DeleteAsync(1), Times.Once);
        _apiClientMock.Verify(x => x.DeleteAsync(2), Times.Once);
    }

    [Fact]
    public async Task DeleteSelectedTasksCommand_WhenNothingSelected_KeepsCollection()
    {
        SetupTasks();

        var vm = CreateViewModel();
        await WaitUntilAsync(() => vm.Tasks.Count == 3);

        await ((AsyncRelayCommand)vm.DeleteSelectedTasksCommand).ExecuteAsync(null);

        Assert.Equal(3, vm.Tasks.Count);

        _apiClientMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteSelectedTasksCommand_WhenException_ShowsErrorAndKeepsTask()
    {
        SetupErrorDialog();

        SetupTasks();

        var vm = CreateViewModel();
        await WaitUntilAsync(() => vm.Tasks.Count == 3);

        vm.Tasks[0].IsSelected = true;

        _apiClientMock.Setup(x => x.DeleteAsync(1)).ThrowsAsync(new Exception());

        await ((AsyncRelayCommand)vm.DeleteSelectedTasksCommand).ExecuteAsync(null);

        Assert.Equal(3, vm.Tasks.Count);
        Assert.Contains(vm.Tasks, x => x.Task.Id == 1);

        _dialogServiceMock.Verify(
            x =>
                x.ShowMessageAsync(
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
        _apiClientMock
            .Setup(x => x.GetTasksCountAsync(1))
            .ReturnsAsync(Result<int>.Success(30));

        _apiClientMock
            .Setup(x =>
                x.GetTasksAsync(
                    1,
                    It.IsAny<string>(),
                    2,
                    It.IsAny<int>(),
                    It.IsAny<List<TaskStatus>>()
                )
            )
            .ReturnsAsync(
                Result<List<TaskInfo>>.Success(new() { CreateTask(4), CreateTask(5) })
            );

        var vm = CreateViewModel();
        await WaitUntilAsync(() => vm.TotalCountOfPages == 2);

        await ((AsyncRelayCommand)vm.NextPageCommand).ExecuteAsync(null);

        Assert.Equal(2, vm.CurrentPage);
        Assert.Equal(2, vm.TotalCountOfPages);
        Assert.Equal(2, vm.Tasks.Count);
    }

    [Fact]
    public async Task NextPageCommand_WhenLastPage_DoesNothing()
    {
        _apiClientMock
            .Setup(x => x.GetTasksCountAsync(1))
            .ReturnsAsync(Result<int>.Success(10));

        var vm = CreateViewModel();
        await WaitUntilAsync(() => vm.TotalCountOfPages == 1);

        _apiClientMock.Invocations.Clear();

        await ((AsyncRelayCommand)vm.NextPageCommand).ExecuteAsync(null);

        Assert.Equal(1, vm.CurrentPage);

        _apiClientMock.Verify(
            x =>
                x.GetTasksAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<List<TaskStatus>>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task SearchFilter_WhenChanged_LoadsTasksWithFilter()
    {
        SetupTasks();

        var vm = CreateViewModel();
        await WaitUntilAsync(() => vm.Tasks.Count == 3);

        _apiClientMock.Invocations.Clear();

        vm.SearchFilter = "test";

        await Task.Delay(100);

        _apiClientMock.Verify(
            x =>
                x.GetTasksAsync(
                    1,
                    "test",
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<List<TaskStatus>>()
                ),
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