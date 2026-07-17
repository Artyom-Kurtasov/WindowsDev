using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using Moq;
using WindowsDev.Business.Services.Localization.Interfaces;
using WindowsDev.Business.Services.ProjectService.Interfaces;
using WindowsDev.Dialogs.Interfaces;
using WindowsDev.Domain;
using WindowsDev.Domain.DialogsMessages.Errors;
using WindowsDev.Domain.ProjectsModels;
using WindowsDev.Infrastructure;
using WindowsDev.NavigationManager.Interfaces;
using WindowsDev.ViewModels.Main.Tabs;
using WindowsDev.ViewModels.Projects;
using WindowsDev.ViewModels.Projects.Dialogs;
using WindowsDev.Views.Projects;

namespace WindowsDev.Tests.ViewModels.Main.Tabs
{
    public class ProjectsViewModelTest
    {
        private readonly Mock<IDialogCoordinator> _dialogCoordinatorMock;
        private readonly Mock<IProjectService> _projectServiceMock;
        private readonly Mock<INavigationService> _navigationServiceMock;
        private readonly Mock<ILogger<ProjectsViewModel>> _loggerMock;
        private readonly Mock<IDialogService> _dialogServiceMock;
        private readonly Mock<ILanguageChanger> _languageChangerMock;

        public ProjectsViewModelTest()
        {
            _dialogCoordinatorMock = new Mock<IDialogCoordinator>();
            _projectServiceMock = new Mock<IProjectService>();
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
                _dialogCoordinatorMock.Object,
                _projectServiceMock.Object,
                _navigationServiceMock.Object,
                _loggerMock.Object,
                _dialogServiceMock.Object,
                _languageChangerMock.Object
            );
        }

        private ProjectsInfo CreateProject(int id, bool selected = false)
        {
            return new ProjectsInfo
            {
                Id = id,
                Name = $"Project{id}",
                UserId = 1,
                CreatedAt = DateTime.Now,
                IsSelected = selected,
            };
        }

        private void SetupErrorDialog()
        {
            _dialogCoordinatorMock
                .Setup(x =>
                    x.ShowMessageAsync(
                        It.IsAny<ProjectsViewModel>(),
                        DialogTitles.Error,
                        CommonErrors.UnexpectedError,
                        MessageDialogStyle.Affirmative
                    )
                )
                .ReturnsAsync(MessageDialogResult.Affirmative);
        }

        [Fact]
        public async Task RefreshAsync_WhenSuccess_LoadsProjects()
        {
            var projects = new List<ProjectsInfo>
            {
                CreateProject(1),
                CreateProject(2),
                CreateProject(3),
            };

            _projectServiceMock.Setup(x => x.GetProjectsCountAsync()).ReturnsAsync(projects.Count);
            _projectServiceMock
                .Setup(x =>
                    x.GetProjectsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())
                )
                .ReturnsAsync(projects);

            var vm = CreateViewModel();

            await vm.RefreshAsync();

            Assert.Equal(3, vm.ProjectsList.Count);
            Assert.Equal(1, vm.CurrentPage);
            Assert.Equal(1, vm.TotalCountOfPages);

            _projectServiceMock.Verify(x => x.GetProjectsCountAsync(), Times.Once);
            _projectServiceMock.Verify(x => x.GetProjectsAsync(1, 15, ""), Times.Once);
        }

        [Fact]
        public async Task RefreshAsync_WhenException_ShowsErrorDialog()
        {
            SetupErrorDialog();

            _projectServiceMock.Setup(x => x.GetProjectsCountAsync()).ThrowsAsync(new Exception());

            var vm = CreateViewModel();

            await vm.RefreshAsync();

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

            Assert.Empty(vm.ProjectsList);
        }

        [Fact]
        public async Task SearchCommand_WhenSuccess_LoadsFilteredProjectsAndResetsPage()
        {
            var projects = new List<ProjectsInfo> { CreateProject(1), CreateProject(2) };

            _projectServiceMock
                .Setup(x => x.GetProjectsAsync(1, 15, "test"))
                .ReturnsAsync(projects);

            var vm = CreateViewModel();
            vm.CurrentPage = 3;
            vm.SearchFilter = "test";

            await ((AsyncRelayCommand)vm.SearchCommand).ExecuteAsync(null);

            Assert.Equal(2, vm.ProjectsList.Count);
            Assert.Equal(1, vm.CurrentPage);

            _projectServiceMock.Verify(x => x.GetProjectsAsync(1, 15, "test"), Times.Once);
        }

        [Fact]
        public async Task SearchCommand_WhenException_ShowsErrorDialog()
        {
            SetupErrorDialog();

            _projectServiceMock
                .Setup(x =>
                    x.GetProjectsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())
                )
                .ThrowsAsync(new Exception());

            var vm = CreateViewModel();
            vm.SearchFilter = "test";

            await ((AsyncRelayCommand)vm.SearchCommand).ExecuteAsync(null);

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

            Assert.Empty(vm.ProjectsList);
        }

        [Fact]
        public async Task DeleteSelectedProjectsCommand_WhenProjectsSelected_DeletesAndRemovesThem()
        {
            var vm = CreateViewModel();

            var selected1 = CreateProject(1, true);
            var selected2 = CreateProject(2, true);
            var normal = CreateProject(3);

            vm.ProjectsList.Add(selected1);
            vm.ProjectsList.Add(selected2);
            vm.ProjectsList.Add(normal);

            await ((AsyncRelayCommand)vm.DeleteSelectedProjectsCommand).ExecuteAsync(null);

            Assert.Single(vm.ProjectsList);
            Assert.Contains(normal, vm.ProjectsList);
            Assert.DoesNotContain(selected1, vm.ProjectsList);
            Assert.DoesNotContain(selected2, vm.ProjectsList);

            _projectServiceMock.Verify(x => x.DeleteAsync(1), Times.Once);
            _projectServiceMock.Verify(x => x.DeleteAsync(2), Times.Once);
        }

        [Fact]
        public async Task DeleteSelectedProjectsCommand_WhenNoProjectsSelected_DoesNotDeleteAnything()
        {
            var vm = CreateViewModel();

            vm.ProjectsList.Add(CreateProject(1));
            vm.ProjectsList.Add(CreateProject(2));

            await ((AsyncRelayCommand)vm.DeleteSelectedProjectsCommand).ExecuteAsync(null);

            Assert.Equal(2, vm.ProjectsList.Count);
            _projectServiceMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task DeleteSelectedProjectsCommand_WhenException_ShowsErrorDialogAndContinuesDeleting()
        {
            SetupErrorDialog();

            var vm = CreateViewModel();

            var selected1 = CreateProject(1, true);
            var selected2 = CreateProject(2, true);
            var selected3 = CreateProject(3, true);

            vm.ProjectsList.Add(selected1);
            vm.ProjectsList.Add(selected2);
            vm.ProjectsList.Add(selected3);

            _projectServiceMock.Setup(x => x.DeleteAsync(1)).ThrowsAsync(new Exception());
            _projectServiceMock.Setup(x => x.DeleteAsync(2)).Returns(Task.CompletedTask);
            _projectServiceMock.Setup(x => x.DeleteAsync(3)).ThrowsAsync(new Exception());

            await ((AsyncRelayCommand)vm.DeleteSelectedProjectsCommand).ExecuteAsync(null);

            _dialogCoordinatorMock.Verify(
                x =>
                    x.ShowMessageAsync(
                        vm,
                        DialogTitles.Error,
                        CommonErrors.UnexpectedError,
                        MessageDialogStyle.Affirmative
                    ),
                Times.Exactly(2)
            );

            _projectServiceMock.Verify(x => x.DeleteAsync(1), Times.Once);
            _projectServiceMock.Verify(x => x.DeleteAsync(2), Times.Once);
            _projectServiceMock.Verify(x => x.DeleteAsync(3), Times.Once);

            Assert.Equal(2, vm.ProjectsList.Count);
            Assert.DoesNotContain(selected2, vm.ProjectsList);
        }

        [Fact]
        public async Task OpenProjectCommand_WhenExecuted_NavigatesToProject()
        {
            var vm = CreateViewModel();
            var project = CreateProject(10);

            await ((AsyncRelayCommandT<ProjectsInfo>)vm.OpenProjectCommand).ExecuteAsync(project);

            _navigationServiceMock.Verify(x => x.NavigateTo<ProjectViewModel>(project), Times.Once);
        }

        [Fact]
        public async Task OpenDialogCommand_WhenExecuted_ShowsCreateProjectDialog()
        {
            var vm = CreateViewModel();

            await ((AsyncRelayCommand)vm.OpenDialogCommand).ExecuteAsync(null);

            _dialogServiceMock.Verify(
                x => x.ShowDialogAsync<CreateProjectDialogView, CreateProjectDialogViewModel>(vm),
                Times.Once
            );
        }

        [Fact]
        public async Task NextPageCommand_WhenCanGoNext_MovesToNextPageAndLoadsProjects()
        {
            var projects = new List<ProjectsInfo> { CreateProject(1), CreateProject(2) };

            _projectServiceMock.Setup(x => x.GetProjectsCountAsync()).ReturnsAsync(30);
            _projectServiceMock.Setup(x => x.GetProjectsAsync(2, 15, "")).ReturnsAsync(projects);

            var vm = CreateViewModel();
            vm.CurrentPage = 1;

            await vm.RefreshAsync();
            await ((AsyncRelayCommand)vm.NextPageCommand).ExecuteAsync(null);

            Assert.Equal(2, vm.CurrentPage);
            Assert.Equal(2, vm.TotalCountOfPages);
            Assert.Equal(2, vm.ProjectsList.Count);

            _projectServiceMock.Verify(x => x.GetProjectsAsync(2, 15, ""), Times.Once);
        }

        [Fact]
        public async Task NextPageCommand_WhenOnLastPage_DoesNotNavigate()
        {
            _projectServiceMock.Setup(x => x.GetProjectsCountAsync()).ReturnsAsync(10);

            var vm = CreateViewModel();
            await vm.RefreshAsync();

            await ((AsyncRelayCommand)vm.NextPageCommand).ExecuteAsync(null);

            Assert.Equal(1, vm.CurrentPage);
            _projectServiceMock.Verify(
                x => x.GetProjectsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()),
                Times.Once
            );
        }

        [Fact]
        public async Task PrevPageCommand_WhenCanGoPrevious_MovesToPreviousPageAndLoadsProjects()
        {
            var projects = new List<ProjectsInfo> { CreateProject(1) };

            _projectServiceMock.Setup(x => x.GetProjectsCountAsync()).ReturnsAsync(30);
            _projectServiceMock.Setup(x => x.GetProjectsAsync(1, 15, "")).ReturnsAsync(projects);

            var vm = CreateViewModel();
            await vm.RefreshAsync();

            vm.CurrentPage = 2;

            await ((AsyncRelayCommand)vm.PrevPageCommand).ExecuteAsync(null);

            Assert.Equal(1, vm.CurrentPage);
            Assert.Equal(2, vm.TotalCountOfPages);
            Assert.Single(vm.ProjectsList);

            _projectServiceMock.Verify(x => x.GetProjectsAsync(1, 15, ""), Times.Exactly(2));
        }

        [Fact]
        public async Task PrevPageCommand_WhenOnFirstPage_DoesNotNavigate()
        {
            _projectServiceMock.Setup(x => x.GetProjectsCountAsync()).ReturnsAsync(30);

            var vm = CreateViewModel();
            vm.CurrentPage = 1;

            await ((AsyncRelayCommand)vm.PrevPageCommand).ExecuteAsync(null);

            Assert.Equal(1, vm.CurrentPage);
            _projectServiceMock.Verify(
                x => x.GetProjectsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()),
                Times.Never
            );
        }

        [Fact]
        public async Task RefreshAsync_WhenSearchFilterNotEmpty_DoesNotUseFilter()
        {
            var projects = new List<ProjectsInfo> { CreateProject(1) };

            _projectServiceMock.Setup(x => x.GetProjectsCountAsync()).ReturnsAsync(1);
            _projectServiceMock.Setup(x => x.GetProjectsAsync(1, 15, "")).ReturnsAsync(projects);

            var vm = CreateViewModel();
            vm.SearchFilter = "test";

            await vm.RefreshAsync();

            _projectServiceMock.Verify(x => x.GetProjectsAsync(1, 15, ""), Times.Once);
            _projectServiceMock.Verify(x => x.GetProjectsAsync(1, 15, "test"), Times.Never);
        }

        [Fact]
        public async Task DeleteSelectedProjectsCommand_WhenExceptionOnFirstProject_ContinuesDeletingOthers()
        {
            SetupErrorDialog();

            var vm = CreateViewModel();

            var selected1 = CreateProject(1, true);
            var selected2 = CreateProject(2, true);
            var selected3 = CreateProject(3, true);

            vm.ProjectsList.Add(selected1);
            vm.ProjectsList.Add(selected2);
            vm.ProjectsList.Add(selected3);

            _projectServiceMock.Setup(x => x.DeleteAsync(1)).ThrowsAsync(new Exception());
            _projectServiceMock.Setup(x => x.DeleteAsync(2)).Returns(Task.CompletedTask);
            _projectServiceMock.Setup(x => x.DeleteAsync(3)).Returns(Task.CompletedTask);

            await ((AsyncRelayCommand)vm.DeleteSelectedProjectsCommand).ExecuteAsync(null);

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

            _projectServiceMock.Verify(x => x.DeleteAsync(1), Times.Once);
            _projectServiceMock.Verify(x => x.DeleteAsync(2), Times.Once);
            _projectServiceMock.Verify(x => x.DeleteAsync(3), Times.Once);

            Assert.Single(vm.ProjectsList);
            Assert.Contains(selected1, vm.ProjectsList);
            Assert.DoesNotContain(selected2, vm.ProjectsList);
            Assert.DoesNotContain(selected3, vm.ProjectsList);
        }
    }
}
