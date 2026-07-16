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
        private readonly Mock<IDialogCoordinator> _dialogCoordinatorMock = new();
        private readonly Mock<IProjectService> _projectServiceMock = new();
        private readonly Mock<INavigationService> _navigationServiceMock = new();
        private readonly Mock<ILogger<ProjectsViewModel>> _loggerMock = new();
        private readonly Mock<IDialogService> _dialogServiceMock = new();
        private readonly Mock<ILanguageChanger> _languageChangerMock = new();


        public ProjectsViewModelTest()
        {
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
                _languageChangerMock.Object);
        }


        private static ProjectsInfo CreateProject(
            int id,
            bool selected = false)
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


        private void SetupErrorDialog()
        {
            _dialogCoordinatorMock
                .Setup(x => x.ShowMessageAsync(
                    It.IsAny<ProjectsViewModel>(),
                    DialogTitles.Error,
                    CommonErrors.UnexpectedError,
                    MessageDialogStyle.Affirmative,
                    It.IsAny<MetroDialogSettings>()))
                .ReturnsAsync(MessageDialogResult.Affirmative);
        }


        [Fact]
        public async Task RefreshAsync_LoadsProjects()
        {
            var projects = new List<ProjectsInfo>
            {
                CreateProject(1),
                CreateProject(2)
            };


            _projectServiceMock
                .Setup(x => x.GetProjectsCountAsync())
                .ReturnsAsync(projects.Count);


            _projectServiceMock
                .Setup(x => x.GetProjectsAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>()))
                .ReturnsAsync(projects);


            var vm = CreateViewModel();


            await vm.RefreshAsync();


            Assert.Equal(2, vm.ProjectsList.Count);
        }


        [Fact]
        public async Task SearchAsync_LoadsFilteredProjects()
        {
            var projects = new List<ProjectsInfo>
            {
                CreateProject(1)
            };


            _projectServiceMock
                .Setup(x => x.GetProjectsAsync(1, 15, "test"))
                .ReturnsAsync(projects);


            var vm = CreateViewModel();

            vm.SearchFilter = "test";


            await ((AsyncRelayCommand)vm.SearchCommand)
                .ExecuteAsync(null);


            Assert.Single(vm.ProjectsList);


            _projectServiceMock.Verify(
                x => x.GetProjectsAsync(1, 15, "test"),
                Times.Once);
        }


        [Fact]
        public async Task DeleteSelectedProjects_RemovesSelected()
        {
            var vm = CreateViewModel();


            var selected = CreateProject(1, true);
            var normal = CreateProject(2);


            vm.ProjectsList.Add(selected);
            vm.ProjectsList.Add(normal);


            await ((AsyncRelayCommand)vm.DeleteSelectedProjectsCommand)
                .ExecuteAsync(null);


            Assert.Single(vm.ProjectsList);
            Assert.Contains(normal, vm.ProjectsList);


            _projectServiceMock.Verify(
                x => x.DeleteAsync(1),
                Times.Once);
        }


        [Fact]
        public async Task DeleteSelectedProjects_WhenException_ShowsError()
        {
            SetupErrorDialog();


            var vm = CreateViewModel();


            vm.ProjectsList.Add(
                CreateProject(1, true));


            _projectServiceMock
                .Setup(x => x.DeleteAsync(1))
                .ThrowsAsync(new Exception());


            await ((AsyncRelayCommand)vm.DeleteSelectedProjectsCommand)
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
        public async Task LoadProjects_WhenException_ShowsError()
        {
            SetupErrorDialog();


            _projectServiceMock
                .Setup(x => x.GetProjectsCountAsync())
                .ThrowsAsync(new Exception());


            var vm = CreateViewModel();


            await vm.RefreshAsync();



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
        public async Task OpenProject_NavigatesToProject()
        {
            var vm = CreateViewModel();

            var project = CreateProject(10);


            await ((AsyncRelayCommandT<ProjectsInfo>)
                vm.OpenProjectCommand)
                .ExecuteAsync(project);



            _navigationServiceMock.Verify(
                x => x.NavigateTo<ProjectViewModel>(project),
                Times.Once);
        }


        [Fact]
        public async Task OpenDialog_ShowsCreateProjectDialog()
        {
            var vm = CreateViewModel();


            await ((AsyncRelayCommand)
                vm.OpenDialogCommand)
                .ExecuteAsync(null);



            _dialogServiceMock.Verify(
                x => x.ShowDialogAsync<
                    CreateProjectDialogView,
                    CreateProjectDialogViewModel>(vm),
                Times.Once);
        }


        [Fact]
        public void SearchFilter_RaisesPropertyChanged()
        {
            var vm = CreateViewModel();

            bool changed = false;


            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(vm.SearchFilter))
                    changed = true;
            };


            vm.SearchFilter = "abc";


            Assert.True(changed);
        }


        [Fact]
        public void CurrentPage_RaisesPropertyChanged()
        {
            var vm = CreateViewModel();

            bool changed = false;


            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(vm.CurrentPage))
                    changed = true;
            };


            vm.CurrentPage = 5;


            Assert.True(changed);
        }
    }
}