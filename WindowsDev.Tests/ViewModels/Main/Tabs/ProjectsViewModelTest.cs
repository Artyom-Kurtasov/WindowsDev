using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using Moq;
using WindowsDev.ApiClient.ProjectsClient;
using WindowsDev.Application.Common.Localization;
using WindowsDev.Application.Primitives;
using WindowsDev.Command;
using WindowsDev.Contracts.UI.Projects;
using WindowsDev.Domain.Messages;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;
using WindowsDev.Services.Dialogs;
using WindowsDev.Services.Navigation;
using WindowsDev.ViewModels.Main.Tabs;
using WindowsDev.ViewModels.Project;
using WindowsDev.ViewModels.Projects.Dialogs;
using WindowsDev.Views.Project;

namespace WindowsDev.Tests.ViewModels.Main.Tabs;

public class ProjectsViewModelTest
{
    private readonly Mock<IDialogService> _dialogServiceMock;
    private readonly Mock<IProjectsApiClient> _projectApiClientMock;
    private readonly Mock<INavigationService> _navigationServiceMock;
    private readonly Mock<ILogger<ProjectsViewModel>> _loggerMock;
    private readonly Mock<ILanguageChanger> _languageChangerMock;

    public ProjectsViewModelTest()
    {
        _projectApiClientMock = new Mock<IProjectsApiClient>();
        _navigationServiceMock = new Mock<INavigationService>();
        _loggerMock = new Mock<ILogger<ProjectsViewModel>>();
        _dialogServiceMock = new Mock<IDialogService>();
        _languageChangerMock = new Mock<ILanguageChanger>();

        _languageChangerMock
            .Setup(x => x.Translate(It.IsAny<string>()))
            .Returns((string key) => key);
    }

    private ProjectsViewModel CreateViewModel()
    {
        return new ProjectsViewModel(
            _navigationServiceMock.Object,
            _loggerMock.Object,
            _dialogServiceMock.Object,
            _languageChangerMock.Object,
            _projectApiClientMock.Object
        );
    }

    private SelectablelProjectViewModel CreateProject(int id, bool isSelected = false)
    {
        var info = new ProjectInfo(id, $"Project{id}", "Description", DateTime.Now);
        return new SelectablelProjectViewModel(info) { IsSelected = isSelected };
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

    [Fact]
    public async Task RefreshAsync_WhenSuccess_LoadsProjects()
    {
        var projects = new List<ProjectInfo>
        {
            new(1, "P1", "D", DateTime.Now),
            new(2, "P2", "D", DateTime.Now),
            new(3, "P3", "D", DateTime.Now),
        };

        _projectApiClientMock
            .Setup(x => x.GetCountAsync())
            .ReturnsAsync(Result<int>.Success(projects.Count));
        _projectApiClientMock
            .Setup(x =>
                x.GetProjectsAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result<List<ProjectInfo>>.Success(projects));

        var vm = CreateViewModel();

        await vm.RefreshAsync();

        Assert.Equal(3, vm.Projects.Count);
        Assert.Equal(1, vm.CurrentPage);
        Assert.Equal(1, vm.TotalCountOfPages);

        _projectApiClientMock.Verify(x => x.GetCountAsync(), Times.Once);
        _projectApiClientMock.Verify(
            x => x.GetProjectsAsync(1, 15, "", It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task RefreshAsync_WhenException_ShowsErrorDialog()
    {
        SetupErrorDialog();

        _projectApiClientMock.Setup(x => x.GetCountAsync()).ThrowsAsync(new Exception());

        var vm = CreateViewModel();

        await vm.RefreshAsync();

        _dialogServiceMock.Verify(
            x =>
                x.ShowMessageAsync(
                    DialogTitles.Error,
                    CommonErrors.UnexpectedError,
                    MessageDialogStyle.Affirmative
                ),
            Times.Once
        );

        Assert.Empty(vm.Projects);
    }

    [Fact]
    public async Task SearchCommand_WhenSuccess_LoadsFilteredProjectsAndResetsPage()
    {
        var projects = new List<ProjectInfo>
        {
            new(1, "P1", "D", DateTime.Now),
            new(2, "P2", "D", DateTime.Now),
        };

        _projectApiClientMock
            .Setup(x => x.GetProjectsAsync(1, 15, "test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<ProjectInfo>>.Success(projects));

        var vm = CreateViewModel();
        vm.CurrentPage = 3;
        vm.SearchFilter = "test";

        await ((AsyncRelayCommand)vm.SearchCommand).ExecuteAsync(null);

        Assert.Equal(2, vm.Projects.Count);
        Assert.Equal(1, vm.CurrentPage);

        _projectApiClientMock.Verify(
            x => x.GetProjectsAsync(1, 15, "test", It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task SearchCommand_WhenException_ShowsErrorDialog()
    {
        SetupErrorDialog();

        _projectApiClientMock
            .Setup(x =>
                x.GetProjectsAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ThrowsAsync(new Exception());

        var vm = CreateViewModel();
        vm.SearchFilter = "test";

        await ((AsyncRelayCommand)vm.SearchCommand).ExecuteAsync(null);

        _dialogServiceMock.Verify(
            x =>
                x.ShowMessageAsync(
                    DialogTitles.Error,
                    CommonErrors.UnexpectedError,
                    MessageDialogStyle.Affirmative
                ),
            Times.Once
        );

        Assert.Empty(vm.Projects);
    }

    [Fact]
    public async Task DeleteSelectedProjectsCommand_WhenProjectsSelected_DeletesAndRemovesThem()
    {
        var vm = CreateViewModel();

        var selected1 = CreateProject(1, true);
        var selected2 = CreateProject(2, true);
        var normal = CreateProject(3);

        vm.Projects.Add(selected1);
        vm.Projects.Add(selected2);
        vm.Projects.Add(normal);

        await ((AsyncRelayCommand)vm.DeleteSelectedProjectsCommand).ExecuteAsync(null);

        Assert.Single(vm.Projects);
        Assert.Contains(normal, vm.Projects);
        Assert.DoesNotContain(selected1, vm.Projects);
        Assert.DoesNotContain(selected2, vm.Projects);

        _projectApiClientMock.Verify(x => x.DeleteAsync(1), Times.Once);
        _projectApiClientMock.Verify(x => x.DeleteAsync(2), Times.Once);
    }

    [Fact]
    public async Task DeleteSelectedProjectsCommand_WhenNoProjectsSelected_DoesNotDeleteAnything()
    {
        var vm = CreateViewModel();

        vm.Projects.Add(CreateProject(1));
        vm.Projects.Add(CreateProject(2));

        await ((AsyncRelayCommand)vm.DeleteSelectedProjectsCommand).ExecuteAsync(null);

        Assert.Equal(2, vm.Projects.Count);
        _projectApiClientMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteSelectedProjectsCommand_WhenException_ShowsErrorDialogAndContinuesDeleting()
    {
        SetupErrorDialog();

        var vm = CreateViewModel();

        var selected1 = CreateProject(1, true);
        var selected2 = CreateProject(2, true);
        var selected3 = CreateProject(3, true);

        vm.Projects.Add(selected1);
        vm.Projects.Add(selected2);
        vm.Projects.Add(selected3);

        _projectApiClientMock.Setup(x => x.DeleteAsync(1)).ThrowsAsync(new Exception());
        _projectApiClientMock.Setup(x => x.DeleteAsync(2)).ReturnsAsync(Result<bool>.Success(true));
        _projectApiClientMock.Setup(x => x.DeleteAsync(3)).ThrowsAsync(new Exception());

        await ((AsyncRelayCommand)vm.DeleteSelectedProjectsCommand).ExecuteAsync(null);

        _dialogServiceMock.Verify(
            x =>
                x.ShowMessageAsync(
                    DialogTitles.Error,
                    CommonErrors.UnexpectedError,
                    MessageDialogStyle.Affirmative
                ),
            Times.Exactly(2)
        );

        _projectApiClientMock.Verify(x => x.DeleteAsync(1), Times.Once);
        _projectApiClientMock.Verify(x => x.DeleteAsync(2), Times.Once);
        _projectApiClientMock.Verify(x => x.DeleteAsync(3), Times.Once);

        Assert.Equal(2, vm.Projects.Count);
        Assert.Contains(selected1, vm.Projects);
        Assert.DoesNotContain(selected2, vm.Projects);
        Assert.Contains(selected3, vm.Projects);
    }

    [Fact]
    public async Task OpenProjectCommand_WhenExecuted_NavigatesToProject()
    {
        var vm = CreateViewModel();
        var projectInfo = new ProjectInfo(10, "P", "D", DateTime.Now);
        var project = new SelectablelProjectViewModel(projectInfo);

        await ((AsyncRelayCommandT<ProjectInfo>)vm.OpenProjectCommand).ExecuteAsync(projectInfo);

        _navigationServiceMock.Verify(x => x.NavigateTo<ProjectViewModel>(projectInfo), Times.Once);
    }

    [Fact]
    public async Task OpenDialogCommand_WhenExecuted_ShowsCreateProjectDialog()
    {
        _dialogServiceMock
            .Setup(x => x.ShowDialogAsync<CreateProjectDialogView, CreateProjectDialogViewModel>())
            .ReturnsAsync(
                new CreateProjectDialogViewModel(
                    _dialogServiceMock.Object,
                    _projectApiClientMock.Object,
                    new Mock<ILogger<CreateProjectDialogViewModel>>().Object,
                    _languageChangerMock.Object
                )
            );
        var vm = CreateViewModel();

        await ((AsyncRelayCommand)vm.OpenDialogCommand).ExecuteAsync(null);

        _dialogServiceMock.Verify(
            x => x.ShowDialogAsync<CreateProjectDialogView, CreateProjectDialogViewModel>(),
            Times.Once
        );
    }

    [Fact]
    public async Task NextPageCommand_WhenCanGoNext_MovesToNextPageAndLoadsProjects()
    {
        var projects = new List<ProjectInfo>
        {
            new(1, "P1", "D", DateTime.Now),
            new(2, "P2", "D", DateTime.Now),
        };

        _projectApiClientMock.Setup(x => x.GetCountAsync()).ReturnsAsync(Result<int>.Success(30));
        _projectApiClientMock
            .Setup(x => x.GetProjectsAsync(2, 15, "", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<ProjectInfo>>.Success(projects));

        var vm = CreateViewModel();
        vm.CurrentPage = 1;

        await vm.RefreshAsync();
        await ((AsyncRelayCommand)vm.NextPageCommand).ExecuteAsync(null);

        Assert.Equal(2, vm.CurrentPage);
        Assert.Equal(2, vm.TotalCountOfPages);
        Assert.Equal(2, vm.Projects.Count);

        _projectApiClientMock.Verify(
            x => x.GetProjectsAsync(2, 15, "", It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task NextPageCommand_WhenOnLastPage_DoesNotNavigate()
    {
        _projectApiClientMock.Setup(x => x.GetCountAsync()).ReturnsAsync(Result<int>.Success(10));

        var vm = CreateViewModel();
        await vm.RefreshAsync();

        await ((AsyncRelayCommand)vm.NextPageCommand).ExecuteAsync(null);

        Assert.Equal(1, vm.CurrentPage);
        _projectApiClientMock.Verify(
            x => x.GetProjectsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task PrevPageCommand_WhenCanGoPrevious_MovesToPreviousPageAndLoadsProjects()
    {
        var projects = new List<ProjectInfo> { new(1, "P1", "D", DateTime.Now) };

        _projectApiClientMock.Setup(x => x.GetCountAsync()).ReturnsAsync(Result<int>.Success(30));
        _projectApiClientMock
            .Setup(x => x.GetProjectsAsync(1, 15, "", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<ProjectInfo>>.Success(projects));

        var vm = CreateViewModel();
        await vm.RefreshAsync();

        vm.CurrentPage = 2;

        await ((AsyncRelayCommand)vm.PrevPageCommand).ExecuteAsync(null);

        Assert.Equal(1, vm.CurrentPage);
        Assert.Equal(2, vm.TotalCountOfPages);
        Assert.Single(vm.Projects);

        _projectApiClientMock.Verify(
            x => x.GetProjectsAsync(1, 15, "", It.IsAny<CancellationToken>()),
            Times.Exactly(2)
        );
    }

    [Fact]
    public async Task PrevPageCommand_WhenOnFirstPage_DoesNotNavigate()
    {
        _projectApiClientMock.Setup(x => x.GetCountAsync()).ReturnsAsync(Result<int>.Success(30));

        var vm = CreateViewModel();
        vm.CurrentPage = 1;

        await ((AsyncRelayCommand)vm.PrevPageCommand).ExecuteAsync(null);

        Assert.Equal(1, vm.CurrentPage);
        _projectApiClientMock.Verify(
            x => x.GetProjectsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task RefreshAsync_WhenSearchFilterNotEmpty_DoesNotUseFilter()
    {
        var projects = new List<ProjectInfo> { new(1, "P1", "D", DateTime.Now) };

        _projectApiClientMock.Setup(x => x.GetCountAsync()).ReturnsAsync(Result<int>.Success(1));
        _projectApiClientMock
            .Setup(x => x.GetProjectsAsync(1, 15, "", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<ProjectInfo>>.Success(projects));

        var vm = CreateViewModel();
        vm.SearchFilter = "test";

        await vm.RefreshAsync();

        _projectApiClientMock.Verify(
            x => x.GetProjectsAsync(1, 15, "", It.IsAny<CancellationToken>()),
            Times.Once
        );
        _projectApiClientMock.Verify(
            x => x.GetProjectsAsync(1, 15, "test", It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task DeleteSelectedProjectsCommand_WhenExceptionOnFirstProject_ContinuesDeletingOthers()
    {
        SetupErrorDialog();

        var vm = CreateViewModel();

        var selected1 = CreateProject(1, true);
        var selected2 = CreateProject(2, true);
        var selected3 = CreateProject(3, true);

        vm.Projects.Add(selected1);
        vm.Projects.Add(selected2);
        vm.Projects.Add(selected3);

        _projectApiClientMock.Setup(x => x.DeleteAsync(1)).ThrowsAsync(new Exception());
        _projectApiClientMock.Setup(x => x.DeleteAsync(2)).ReturnsAsync(Result<bool>.Success(true));
        _projectApiClientMock.Setup(x => x.DeleteAsync(3)).ReturnsAsync(Result<bool>.Success(true));

        await ((AsyncRelayCommand)vm.DeleteSelectedProjectsCommand).ExecuteAsync(null);

        _dialogServiceMock.Verify(
            x =>
                x.ShowMessageAsync(
                    DialogTitles.Error,
                    CommonErrors.UnexpectedError,
                    MessageDialogStyle.Affirmative
                ),
            Times.Once
        );

        _projectApiClientMock.Verify(x => x.DeleteAsync(1), Times.Once);
        _projectApiClientMock.Verify(x => x.DeleteAsync(2), Times.Once);
        _projectApiClientMock.Verify(x => x.DeleteAsync(3), Times.Once);

        Assert.Single(vm.Projects);
        Assert.Contains(selected1, vm.Projects);
        Assert.DoesNotContain(selected2, vm.Projects);
        Assert.DoesNotContain(selected3, vm.Projects);
    }
}