using System.Collections.ObjectModel;
using System.Windows.Input;
using WindowsDev.Business.Services;
using WindowsDev.Business.Services.ProjectService.Interfaces;
using WindowsDev.Commands.NavigationManager.Interfaces;
using WindowsDev.Domain.UsersAuthInfo;
using WindowsDev.Infrastructure;
using WindowsDev.ViewModels.Interfaces;
using WindowsDev.ViewModels.Projects;
using WindowsDev.Views.Projects;

namespace WindowsDev.ViewModels.Main.Tabs
{
    public class ProjectsViewModel : ViewModelBase, IInitializableAsync
    {
        private readonly IProjectWriter _projectWriter;
        private readonly IProjectLoader _projectLoader;
        private readonly INavigationService _navigationService;
        private readonly DialogShowingService _dialogShowingService;

        public ObservableCollection<ProjectsInfo> ProjectsList { get; } = new();

        /// <summary>
        /// Command to delete all selected projects.
        /// </summary>
        public ICommand DeleteSelectedProjectsCommand { get; }

        /// <summary>
        /// Command to open a project creation dialog.
        /// </summary>
        public ICommand OpenDialogCommand { get; }

        /// <summary>
        /// Command to open a selected project.
        /// </summary>
        public ICommand OpenProjectCommand { get; }

        public ProjectsViewModel(DialogShowingService dialogShowingService, INavigationService navigationService,
            IProjectLoader projectLoader, IProjectWriter projectWriter)
        {
            _navigationService = navigationService;
            _dialogShowingService = dialogShowingService;
            _projectLoader = projectLoader;
            _projectWriter = projectWriter;

            DeleteSelectedProjectsCommand = new AsyncRelayCommand(DeleteSelectedProjects);
            OpenDialogCommand = new AsyncRelayCommand(ShowCreateProjectDialog);
            OpenProjectCommand = new AsyncRelayCommand<ProjectsInfo>(OpenProject, _ => true);
        }

        /// <summary>
        /// Loads the list of projects asynchronously when the ViewModel is initialized.
        /// </summary>
        public async Task InitializationAsync(params object[] parameters)
        {
            ProjectsList.Clear();

            var projects = await _projectLoader.LoadProjectAsync();

            foreach (var project in projects)
            {
                ProjectsList.Add(project);
            }
        }

        /// <summary>
        /// Deletes all selected projects from the project list and database.
        /// </summary>
        private async Task DeleteSelectedProjects()
        {
            var projectsToDelete = ProjectsList?.Where(x => x.IsSelected).ToList();

            if (projectsToDelete != null && projectsToDelete.Any())
            {
                foreach (var project in projectsToDelete)
                {
                    await _projectWriter.DeleteAsync(project.Id);
                    ProjectsList?.Remove(project);
                }
            }
        }

        /// <summary>
        /// Opens the selected project and loads its tasks asynchronously.
        /// </summary>
        private async Task OpenProject(ProjectsInfo project)
        {
            await _navigationService.NavigateTo<ProjectViewModel>(project);
        }

        /// <summary>
        /// Shows the dialog to create a new project.
        /// </summary>
        private async Task ShowCreateProjectDialog()
        {
            await _dialogShowingService.ShowCreateDialogAsync<CreateProjectView, DialogsViewModel>(this, null);
        }
    }
}
