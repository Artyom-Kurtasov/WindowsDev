using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows.Input;
using WindowsDev.ApiClient.ProjectsClient;
using WindowsDev.Application.Common.Localization;
using WindowsDev.Command;
using WindowsDev.Contracts.Api.Projects;
using WindowsDev.Domain.Messages;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;
using WindowsDev.Infrastructure.Logging;
using WindowsDev.Services.Dialogs;
using WindowsDev.Services.Navigation;
using WindowsDev.ViewModels.Interfaces;
using WindowsDev.ViewModels.Project;
using WindowsDev.ViewModels.Projects.Dialogs;
using WindowsDev.Views.Project;

namespace WindowsDev.ViewModels.Main.Tabs;

internal class ProjectsViewModel : LocalizedViewModelBase, IRefreshableViewModel
{
    private readonly IProjectsApiClient _projectsApiClient;
    private readonly INavigationService _navigationService;
    private readonly ILogger<ProjectsViewModel> _logger;
    private readonly IDialogService _dialogService;

    private const int PageSize = 15;

    public ProjectsViewModel(
        INavigationService navigationService,
        ILogger<ProjectsViewModel> logger,
        IDialogService dialogService,
        ILanguageChanger languageChanger,
        IProjectsApiClient projectsApiClient
    )
        : base(languageChanger)
    {
        _navigationService = navigationService;
        _logger = logger;
        _dialogService = dialogService;
        _projectsApiClient = projectsApiClient;

        DeleteSelectedProjectsCommand = new AsyncRelayCommand(DeleteSelectedProjectsAsync);
        OpenDialogCommand = new AsyncRelayCommand(ShowCreateProjectDialogAsync);
        OpenProjectCommand = new AsyncRelayCommandT<GetProjectsResponse>(OpenProjectAsync);
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

    public ObservableCollection<SelectablelProjectViewModel> Projects { get; } = new();

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
            var result = await _projectsApiClient.GetCountAsync();

            if (result.IsFailure)
            {
                await _dialogService.ShowMessageAsync(DialogTitles.Error, result.Error, MessageDialogStyle.Affirmative);
                return;
            }

            _totalCountOfProjects = result.Value.TotalCount;

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
        var projectsToDelete = Projects.Where(x => x.IsSelected).ToList();

        if (!projectsToDelete.Any())
            return;

        foreach (var project in projectsToDelete)
        {
            try
            {
                await _projectsApiClient.DeleteAsync(project.Project.Id);
                Projects.Remove(project);
            }
            catch (Exception ex)
            {
                ProjectLogs.ProjectDeleteFailed(_logger, project.Project.Id, ex);

                await ShowErrorDialogAsync();
            }
        }
    }

    private async Task OpenProjectAsync(GetProjectsResponse project)
    {
        await _navigationService.NavigateTo<ProjectViewModel>(project);
    }

    private async Task ShowCreateProjectDialogAsync()
    {
        await _dialogService.ShowDialogAsync<CreateProjectDialogView, CreateProjectDialogViewModel>(
        );
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
            Projects.Clear();

            var projects = await _projectsApiClient.GetProjectsAsync(
                CurrentPage,
                PageSize,
                searchFilter
            );

            foreach (var project in projects.Value)
            {
                Projects.Add(new SelectablelProjectViewModel(project));
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
        await _dialogService.ShowMessageAsync(
            Translate(DialogTitles.Error),
            Translate(CommonErrors.UnexpectedError),
            MessageDialogStyle.Affirmative
        );
    }
}
