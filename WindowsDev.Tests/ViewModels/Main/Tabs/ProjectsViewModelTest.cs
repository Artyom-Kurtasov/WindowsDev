using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using Moq;
using WindowsDev.Business.Services.ProjectService.Interfaces;
using WindowsDev.Commands.NavigationManager.Interfaces;
using WindowsDev.Dialogs.Interfaces;
using WindowsDev.Domain.ProjectsModels;
using WindowsDev.Infrastructure;
using WindowsDev.ViewModels.Main.Tabs;
using WindowsDev.ViewModels.Projects;
using WindowsDev.ViewModels.Projects.Dialogs;
using WindowsDev.Views.Projects;

namespace WindowsDev.Tests.ViewModels.Main.Tabs
{
    public class ProjectsViewModelTest
    {
        private readonly Mock<IDialogCoordinator> _dialogCoordinatorMock = new();
        private readonly Mock<IProjectService> _projectServiceMock = new();
        private readonly Mock<INavigationService> _navigationServiceMock = new();
        private readonly Mock<ILogger<ProjectsViewModel>> _loggerMock = new();
        private readonly Mock<IDialogService> _dialogServiceMock = new();

        private ProjectsViewModel CreateViewModel()
        {
            return new ProjectsViewModel(
                _dialogCoordinatorMock.Object,
                _projectServiceMock.Object,
                _navigationServiceMock.Object,
                _loggerMock.Object,
                _dialogServiceMock.Object);
        }

        private static ProjectsInfo CreateProject(int id, bool selected = false)
        {
            return new ProjectsInfo
            {
                Id = id,
                Name = $"Project{id}",
                UserId = 1,
                CreatedAt = DateTime.Now,
                IsSelected = selected
            };
        }

        // Initialization

        [Fact]
        public async Task InitializationAsync_LoadsProjects()
        {
            var projects = new List<ProjectsInfo>
            {
                CreateProject(1),
                CreateProject(2),
                CreateProject(3)
            };

            _projectServiceMock
                .Setup(x => x.GetProjectsCountAsync())
                .ReturnsAsync(projects.Count);

            _projectServiceMock
                .Setup(x => x.GetProjectsAsync(1, 10, ""))
                .ReturnsAsync(projects);

            var vm = CreateViewModel();

            await vm.InitializationAsync();

            Assert.Equal(projects.Count, vm.ProjectsList.Count);

            _projectServiceMock.Verify(
                x => x.GetProjectsAsync(1, 10, ""),
                Times.Once);
        }

        // Search

        [Fact]
        public async Task SearchAsync_ResetsPageAndLoadsData()
        {
            _projectServiceMock
                .Setup(x => x.GetProjectsAsync(1, 10, "test"))
                .ReturnsAsync(new List<ProjectsInfo>
                {
                    CreateProject(1),
                    CreateProject(2)
                });

            var vm = CreateViewModel();

            vm.SearchFilter = "test";

            await ((AsyncRelayCommand)vm.SearchCommand)
                .ExecuteAsync(null);

            Assert.Equal(1, vm.CurrentPage);

            _projectServiceMock.Verify(
                x => x.GetProjectsAsync(1, 10, "test"),
                Times.Once);
        }

        // Delete

        [Fact]
        public async Task DeleteSelectedProjectsAsync_DeletesOnlySelected()
        {
            var vm = CreateViewModel();

            var p1 = CreateProject(1, true);
            var p2 = CreateProject(2, false);

            vm.ProjectsList.Add(p1);
            vm.ProjectsList.Add(p2);

            await ((AsyncRelayCommand)vm.DeleteSelectedProjectsCommand)
                .ExecuteAsync(null);

            Assert.Single(vm.ProjectsList);
            Assert.Contains(p2, vm.ProjectsList);
            Assert.DoesNotContain(p1, vm.ProjectsList);

            _projectServiceMock.Verify(x => x.DeleteAsync(1), Times.Once);
            _projectServiceMock.Verify(x => x.DeleteAsync(2), Times.Never);
        }

        [Fact]
        public async Task DeleteSelectedProjectsAsync_WhenNothingSelected_DoesNothing()
        {
            var vm = CreateViewModel();

            vm.ProjectsList.Add(CreateProject(1, false));

            await ((AsyncRelayCommand)vm.DeleteSelectedProjectsCommand)
                .ExecuteAsync(null);

            Assert.Single(vm.ProjectsList);

            _projectServiceMock.Verify(
                x => x.DeleteAsync(It.IsAny<int>()),
                Times.Never);
        }

        // Pagination

        [Fact]
        public async Task NextPageAsync_IncrementsPageAndLoadsData()
        {
            var vm = CreateViewModel();
            vm.CurrentPage = 1;

            _projectServiceMock
                .Setup(x => x.GetProjectsCountAsync())
                .ReturnsAsync(20);

            await vm.InitializationAsync();

            _projectServiceMock
                .Setup(x => x.GetProjectsAsync(2, 10, ""))
                .ReturnsAsync(new List<ProjectsInfo> { CreateProject(1) });

            await ((AsyncRelayCommand)vm.NextPageCommand)
                .ExecuteAsync(null);

            Assert.Equal(2, vm.CurrentPage);

            _projectServiceMock.Verify(
                x => x.GetProjectsAsync(2, 10, ""),
                Times.Once);
        }

        [Fact]
        public async Task PrevPageAsync_DecrementsPageAndLoadsData()
        {
            var vm = CreateViewModel();
            vm.CurrentPage = 2;

            _projectServiceMock
                .Setup(x => x.GetProjectsAsync(1, 10, ""))
                .ReturnsAsync(new List<ProjectsInfo> { CreateProject(1) });

            await ((AsyncRelayCommand)vm.PrevPageCommand)
                .ExecuteAsync(null);

            Assert.Equal(1, vm.CurrentPage);

            _projectServiceMock.Verify(
                x => x.GetProjectsAsync(1, 10, ""),
                Times.Once);
        }

        [Fact]
        public async Task PrevPageAsync_OnFirstPage_DoesNothing()
        {
            var vm = CreateViewModel();
            vm.CurrentPage = 1;

            await ((AsyncRelayCommand)vm.PrevPageCommand)
                .ExecuteAsync(null);

            _projectServiceMock.Verify(
                x => x.GetProjectsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()),
                Times.Never);
        }

        // Navigation

        [Fact]
        public async Task OpenProjectAsync_NavigatesToProject()
        {
            var vm = CreateViewModel();

            var project = CreateProject(10);

            await ((AsyncRelayCommandT<ProjectsInfo>)vm.OpenProjectCommand)
                .ExecuteAsync(project);

            _navigationServiceMock.Verify(
                x => x.NavigateTo<ProjectViewModel>(project),
                Times.Once);
        }

        // Dialog

        [Fact]
        public async Task OpenDialogAsync_ShowsCreateProjectDialog()
        {
            var vm = CreateViewModel();

            await ((AsyncRelayCommand)vm.OpenDialogCommand)
                .ExecuteAsync(null);

            _dialogServiceMock.Verify(
                x => x.ShowTaskDialogAsync<CreateProjectView, CreateProjectDialogViewModel>(vm),
                Times.Once);
        }
    }
}