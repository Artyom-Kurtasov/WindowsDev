using System.Collections.ObjectModel;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using WindowsDev.Business.Services.Localization.Interfaces;
using WindowsDev.Business.Services.ProjectService.Interfaces;
using WindowsDev.Dialogs.Interfaces;
using WindowsDev.Domain;
using WindowsDev.Domain.DialogsMessages.Errors;
using WindowsDev.Domain.ProjectsModels;
using WindowsDev.Infrastructure;
using WindowsDev.Infrastructure.Logging;
using WindowsDev.NavigationManager.Interfaces;
using WindowsDev.ViewModels.Interfaces;
using WindowsDev.ViewModels.Projects;
using WindowsDev.ViewModels.Projects.Dialogs;
using WindowsDev.Views.Projects;

namespace WindowsDev.ViewModels.Main.Tabs
{
    public class ProjectsViewModel : LocalizedViewModelBase, IRefreshableViewModel
    {
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly IProjectService _projectService;
        private readonly INavigationService _navigationService;
        private readonly ILogger<ProjectsViewModel> _logger;
        private readonly IDialogService _dialogService;

        private const int PageSize = 15;

        public ProjectsViewModel(
            IDialogCoordinator dialogCoordinator,
            IProjectService projectService,
            INavigationService navigationService,
            ILogger<ProjectsViewModel> logger,
            IDialogService dialogService,
            ILanguageChanger languageChanger
        )
            : base(languageChanger)
        {
            _dialogCoordinator = dialogCoordinator;
            _projectService = projectService;
            _navigationService = navigationService;
            _logger = logger;
            _dialogService = dialogService;

            DeleteSelectedProjectsCommand = new AsyncRelayCommand(DeleteSelectedProjectsAsync);
            OpenDialogCommand = new AsyncRelayCommand(ShowCreateProjectDialogAsync);
            OpenProjectCommand = new AsyncRelayCommandT<ProjectsInfo>(OpenProjectAsync);
            SearchCommand = new AsyncRelayCommand(SearchAsync);
            NextPageCommand = new AsyncRelayCommand(NextPageAsync);
            PrevPageCommand = new AsyncRelayCommand(PrevPageAsync);
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
                if (_searchFilter == value)
                    return;

                _searchFilter = value;

                OnPropertyChanged();
            }
        }

        private int _currentPage = 1;

        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (_currentPage == value)
                    return;

                _currentPage = value;

                OnPropertyChanged();
            }
        }

        private int _totalCountOfProjects;

        public int TotalCountOfPages => (int)Math.Ceiling((double)_totalCountOfProjects / PageSize);

        public async Task RefreshAsync()
        {
            await LoadProjectsAsync();
        }

        private async Task LoadProjectsAsync()
        {
            try
            {
                _totalCountOfProjects = await _projectService.GetProjectsCountAsync();

                OnPropertyChanged(nameof(TotalCountOfPages));

                await GetPageAsync();
            }
            catch (Exception ex)
            {
                ProjectLogs.ProjectLoadFailed(_logger, ex);

                await ShowErrorDialogAsync();
            }
        }

        private async Task SearchAsync()
        {
            CurrentPage = 1;

            await GetPageAsync(SearchFilter);
        }

        private async Task DeleteSelectedProjectsAsync()
        {
            var projectsToDelete = ProjectsList.Where(x => x.IsSelected).ToList();

            foreach (var project in projectsToDelete)
            {
                try
                {
                    await _projectService.DeleteAsync(project.Id);
                    ProjectsList.Remove(project);
                }
                catch (Exception ex)
                {
                    ProjectLogs.ProjectDeleteFailed(_logger, project.Id, ex);

                    await ShowErrorDialogAsync();
                }
            }
        }

        private async Task OpenProjectAsync(ProjectsInfo project)
        {
            await _navigationService.NavigateTo<ProjectViewModel>(project);
        }

        private async Task ShowCreateProjectDialogAsync()
        {
            await _dialogService.ShowDialogAsync<
                CreateProjectDialogView,
                CreateProjectDialogViewModel
            >(this);
        }

        private async Task NextPageAsync()
        {
            if (CurrentPage >= TotalCountOfPages)
                return;

            CurrentPage++;

            await GetPageAsync(SearchFilter);
        }

        private async Task PrevPageAsync()
        {
            if (CurrentPage <= 1)
                return;

            CurrentPage--;

            await GetPageAsync(SearchFilter);
        }

        private async Task GetPageAsync(string searchFilter = "")
        {
            try
            {
                ProjectsList.Clear();

                var projects = await _projectService.GetProjectsAsync(
                    CurrentPage,
                    PageSize,
                    searchFilter
                );

                foreach (var project in projects)
                {
                    ProjectsList.Add(project);
                }
            }
            catch (Exception ex)
            {
                ProjectLogs.ProjectLoadFailed(_logger, ex);

                await ShowErrorDialogAsync();
            }
        }

        private async Task ShowErrorDialogAsync()
        {
            await _dialogCoordinator.ShowMessageAsync(
                this,
                Translate(DialogTitles.Error),
                Translate(CommonErrors.UnexpectedError),
                MessageDialogStyle.Affirmative
            );
        }
    }
}
