using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows.Input;
using WindowsDev.Business.Services.ProjectService.Interfaces;
using WindowsDev.Dialogs.Interfaces;
using WindowsDev.Domain.ProjectsModels;
using WindowsDev.Infrastructure;
using WindowsDev.NavigationManager.Interfaces;
using WindowsDev.ViewModels.Interfaces;
using WindowsDev.ViewModels.Projects;
using WindowsDev.ViewModels.Projects.Dialogs;
using WindowsDev.Views.Projects;

namespace WindowsDev.ViewModels.Main.Tabs
{
    public class ProjectsViewModel : ViewModelBase, IRefreshableViewModel
    {
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly IProjectService _projectService;
        private readonly INavigationService _navigationService;
        private readonly ILogger<ProjectsViewModel> _logger;
        private readonly IDialogService _dialogService;

        private readonly int _pageSize = 15;

        public ProjectsViewModel(IDialogCoordinator dialogCoordinator,
            IProjectService projectService,
            INavigationService navigationService,
            ILogger<ProjectsViewModel> logger,
            IDialogService dialogService)
        {
            _navigationService = navigationService;
            _dialogService = dialogService;
            _projectService = projectService;
            _dialogCoordinator = dialogCoordinator;
            _logger = logger;

            DeleteSelectedProjectsCommand = new AsyncRelayCommand(DeleteSelectedProjectsAsync);
            OpenDialogCommand = new AsyncRelayCommand(ShowCreateProjectDialogAsync);
            OpenProjectCommand = new AsyncRelayCommandT<ProjectsInfo>(OpenProjectAsync, _ => true);
            SearchCommand = new AsyncRelayCommand(SearchAsync);
            NextPageCommand = new AsyncRelayCommand(NextPageAsync);
            PrevPageCommand = new AsyncRelayCommand(PrevPageAsync);

            _ = LoadProjectsAsync();
        }

        public ICommand DeleteSelectedProjectsCommand { get; }
        public ICommand OpenDialogCommand { get; }
        public ICommand OpenProjectCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PrevPageCommand { get; }
        public ICommand SearchCommand { get; }

        public ObservableCollection<ProjectsInfo> ProjectsList { get; } = new();

        private string _searchFilter = string.Empty;
        public string SearchFilter
        {
            get => _searchFilter;
            set
            {
                _searchFilter = value;
                OnPropertyChanged(nameof(SearchFilter));
            }
        }

        private int _currentPage = 1;
        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
            }
        }

        private int _totalCountOfProjects;
        public int TotalCountOfPages => (int)Math.Ceiling((double)_totalCountOfProjects / _pageSize);

        public async Task RefreshAsync()
        {
            await LoadProjectsAsync();
        }

        private async Task LoadProjectsAsync()
        {
            _totalCountOfProjects = await _projectService.GetProjectsCountAsync();
            OnPropertyChanged(nameof(TotalCountOfPages));
            await GetPageAsync();
        }

        private async Task SearchAsync()
        {
            // Reset to first page when search criteria change
            // otherwise we might land on an empty page
            CurrentPage = 1;
            await GetPageAsync(SearchFilter);
        }

        private async Task DeleteSelectedProjectsAsync()
        {
            try
            {
                var projectsToDelete = ProjectsList
                    .Where(x => x.IsSelected)
                    .ToList();

                foreach (var project in projectsToDelete)
                {
                    await _projectService.DeleteAsync(project.Id);
                    ProjectsList.Remove(project);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete projects");
                await _dialogCoordinator.ShowMessageAsync(this,
                    Translate("Error_Title"),
                    Translate("Error_DeleteProjects"),
                    MessageDialogStyle.Affirmative);
            }
        }

        private async Task OpenProjectAsync(ProjectsInfo project) =>
            await _navigationService.NavigateTo<ProjectViewModel>(project);

        private async Task ShowCreateProjectDialogAsync() =>
            await _dialogService.ShowDialogAsync<CreateProjectDialogView, CreateProjectDialogViewModel>(this);

        private async Task NextPageAsync()
        {
            if (CurrentPage < TotalCountOfPages)
            {
                CurrentPage++;
                await GetPageAsync(SearchFilter);
            }
        }

        private async Task PrevPageAsync()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                await GetPageAsync(SearchFilter);
            }
        }

        private async Task GetPageAsync(string searchFilter = "")
        {
            ProjectsList.Clear();

            var projects = await _projectService
                .GetProjectsAsync(CurrentPage, _pageSize, searchFilter);

            foreach (var project in projects)
            {
                ProjectsList.Add(project);
            }

        }
    }
}
