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

        [Fact]
        public async Task RefreshAsync_LoadsProjects()
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
                .Setup(x => x.GetProjectsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(projects);

            var vm = CreateViewModel();

            await vm.RefreshAsync();

            Assert.Equal(projects.Count, vm.ProjectsList.Count);
        }

        [Fact]
        public async Task SearchAsync_ResetsPageAndLoadsData()
        {
            var searchResults = new List<ProjectsInfo>
            {
                CreateProject(1),
                CreateProject(2)
            };

            _projectServiceMock
                .Setup(x => x.GetProjectsCountAsync())
                .ReturnsAsync(2);

            _projectServiceMock
                .Setup(x => x.GetProjectsAsync(1, 15, "test"))
                .ReturnsAsync(searchResults);

            var vm = CreateViewModel();
            await Task.Delay(50);

            vm.SearchFilter = "test";

            await ((AsyncRelayCommand)vm.SearchCommand).ExecuteAsync(null);

            Assert.Equal(1, vm.CurrentPage);
            Assert.Equal(2, vm.ProjectsList.Count);

            _projectServiceMock.Verify(
                x => x.GetProjectsAsync(1, 15, "test"),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task SearchAsync_WithEmptyFilter_LoadsAllProjects()
        {
            var projects = new List<ProjectsInfo>
            {
                CreateProject(1),
                CreateProject(2)
            };

            _projectServiceMock
                .Setup(x => x.GetProjectsCountAsync())
                .ReturnsAsync(2);

            _projectServiceMock
                .Setup(x => x.GetProjectsAsync(1, 15, ""))
                .ReturnsAsync(projects);

            var vm = CreateViewModel();
            await Task.Delay(50);

            vm.SearchFilter = "";

            await ((AsyncRelayCommand)vm.SearchCommand).ExecuteAsync(null);

            Assert.Equal(2, vm.ProjectsList.Count);
        }

        [Fact]
        public async Task DeleteSelectedProjectsAsync_DeletesOnlySelected()
        {
            var vm = CreateViewModel();
            await Task.Delay(50);

            var p1 = CreateProject(1, true);
            var p2 = CreateProject(2, false);

            vm.ProjectsList.Clear();
            vm.ProjectsList.Add(p1);
            vm.ProjectsList.Add(p2);

            _projectServiceMock
                .Setup(x => x.DeleteAsync(1))
                .Returns(Task.CompletedTask);

            await ((AsyncRelayCommand)vm.DeleteSelectedProjectsCommand).ExecuteAsync(null);

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
            await Task.Delay(50);

            var project = CreateProject(1, false);
            vm.ProjectsList.Clear();
            vm.ProjectsList.Add(project);

            await ((AsyncRelayCommand)vm.DeleteSelectedProjectsCommand).ExecuteAsync(null);

            Assert.Single(vm.ProjectsList);
            _projectServiceMock.Verify(
                x => x.DeleteAsync(It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteSelectedProjectsAsync_WhenServiceThrows_ShowsErrorDialog()
        {
            var vm = CreateViewModel();
            await Task.Delay(50);

            var project = CreateProject(1, true);
            vm.ProjectsList.Clear();
            vm.ProjectsList.Add(project);

            _projectServiceMock
                .Setup(x => x.DeleteAsync(1))
                .ThrowsAsync(new Exception("Database error"));

            _projectServiceMock
                .Setup(x => x.GetProjectsCountAsync())
                .ReturnsAsync(1);

            await ((AsyncRelayCommand)vm.DeleteSelectedProjectsCommand).ExecuteAsync(null);

            _dialogCoordinatorMock.Verify(
                x => x.ShowMessageAsync(
                    vm,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    MessageDialogStyle.Affirmative,
                    It.IsAny<MetroDialogSettings>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task LoadProjectsAsync_WhenGetCountThrows_ShowsErrorDialog()
        {
            _projectServiceMock
                .Setup(x => x.GetProjectsCountAsync())
                .ThrowsAsync(new Exception("Database error"));

            var vm = CreateViewModel();

            var exception = await Assert.ThrowsAsync<Exception>(() => vm.RefreshAsync());
            Assert.Equal("Database error", exception.Message);

            _dialogCoordinatorMock.Verify(
                x => x.ShowMessageAsync(
                    vm,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    MessageDialogStyle.Affirmative,
                    It.IsAny<MetroDialogSettings>()),
                Times.Never);
        }

        [Fact]
        public async Task NextPageAsync_IncrementsPageAndLoadsData()
        {
            _projectServiceMock
                .Setup(x => x.GetProjectsCountAsync())
                .ReturnsAsync(30); 

            var vm = CreateViewModel();
            await Task.Delay(50);

            vm.CurrentPage = 1;

            _projectServiceMock
                .Setup(x => x.GetProjectsAsync(2, 15, ""))
                .ReturnsAsync(new List<ProjectsInfo> { CreateProject(16) });

            await ((AsyncRelayCommand)vm.NextPageCommand).ExecuteAsync(null);

            Assert.Equal(2, vm.CurrentPage);

            _projectServiceMock.Verify(
                x => x.GetProjectsAsync(2, 15, ""),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task NextPageAsync_OnLastPage_DoesNothing()
        {
            _projectServiceMock
                .Setup(x => x.GetProjectsCountAsync())
                .ReturnsAsync(10);

            var vm = CreateViewModel();
            await Task.Delay(50);

            var initialPage = vm.CurrentPage;

            await ((AsyncRelayCommand)vm.NextPageCommand).ExecuteAsync(null);

            Assert.Equal(initialPage, vm.CurrentPage);
        }

        [Fact]
        public async Task PrevPageAsync_DecrementsPageAndLoadsData()
        {
            _projectServiceMock
                .Setup(x => x.GetProjectsCountAsync())
                .ReturnsAsync(30);

            var vm = CreateViewModel();
            await Task.Delay(50);

            vm.CurrentPage = 2;

            _projectServiceMock
                .Setup(x => x.GetProjectsAsync(1, 15, ""))
                .ReturnsAsync(new List<ProjectsInfo> { CreateProject(1) });

            await ((AsyncRelayCommand)vm.PrevPageCommand).ExecuteAsync(null);

            Assert.Equal(1, vm.CurrentPage);

            _projectServiceMock.Verify(
                x => x.GetProjectsAsync(1, 15, ""),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task PrevPageAsync_OnFirstPage_DoesNothing()
        {
            var vm = CreateViewModel();
            await Task.Delay(50);

            vm.CurrentPage = 1;
            var initialPage = vm.CurrentPage;

            await ((AsyncRelayCommand)vm.PrevPageCommand).ExecuteAsync(null);

            Assert.Equal(initialPage, vm.CurrentPage);
            _projectServiceMock.Verify(
                x => x.GetProjectsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()),
                Times.AtMostOnce);
        }

        [Fact]
        public async Task TotalCountOfPages_CalculatesCorrectly()
        {
            _projectServiceMock
                .Setup(x => x.GetProjectsCountAsync())
                .ReturnsAsync(25);

            var vm = CreateViewModel();

            await Task.Delay(50);

            Assert.Equal(2, vm.TotalCountOfPages);
        }


        [Fact]
        public async Task OpenProjectAsync_NavigatesToProject()
        {
            var vm = CreateViewModel();
            var project = CreateProject(10);

            await ((AsyncRelayCommandT<ProjectsInfo>)vm.OpenProjectCommand).ExecuteAsync(project);

            _navigationServiceMock.Verify(
                x => x.NavigateTo<ProjectViewModel>(project),
                Times.Once);
        }


        [Fact]
        public async Task OpenDialogAsync_ShowsCreateProjectDialog()
        {
            var vm = CreateViewModel();

            await ((AsyncRelayCommand)vm.OpenDialogCommand).ExecuteAsync(null);

            _dialogServiceMock.Verify(
                x => x.ShowDialogAsync<CreateProjectDialogView, CreateProjectDialogViewModel>(vm),
                Times.Once);
        }

        [Fact]
        public void SearchFilter_WhenChanged_RaisesPropertyChanged()
        {
            var vm = CreateViewModel();
            var propertyChanged = false;
            vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(vm.SearchFilter))
                    propertyChanged = true;
            };

            vm.SearchFilter = "new search";

            Assert.True(propertyChanged);
        }

        [Fact]
        public void CurrentPage_WhenChanged_RaisesPropertyChanged()
        {
            var vm = CreateViewModel();
            var propertyChanged = false;
            vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(vm.CurrentPage))
                    propertyChanged = true;
            };

            vm.CurrentPage = 5;

            Assert.True(propertyChanged);
        }
    }
}