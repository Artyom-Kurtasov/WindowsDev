using System.Collections.ObjectModel;
using System.Windows.Input;
using WindowsDev.Business.DataBase;
using WindowsDev.Business.Services;
using WindowsDev.Business.Services.Localization;
using WindowsDev.Business.Services.ProjectService;
using WindowsDev.Business.Services.ProjectService.Interfaces;
using WindowsDev.Business.Services.TaskService.Interfaces;
using WindowsDev.Commands.NavigationManager;
using WindowsDev.Commands.NavigationManager.Interfaces;
using WindowsDev.Domain.UsersAuthInfo;
using WindowsDev.Infrastructure;
using WindowsDev.Settings;
using WindowsDev.View.Controls.Users;

namespace WindowsDev.ViewModels
{
    /// <summary>
    /// ViewModel for the main window, handles navigation, project list, and dialogs.
    /// Implements IDisposable to unsubscribe from events.
    /// </summary>
    public class MainWindowViewModel : ViewModelBase, IDisposable
    {
        private readonly SharedDataService _sharedDataService;
        private readonly DialogShowingService _creator;
        private readonly NavigationStore _navigationStore;
        private readonly LanguageChanger _languageChanger;
        private readonly INavigationService _navigationService;
        private readonly ITaskLoader _taskLoader;
        private readonly DbManager _dbManager;
        private readonly IProjectWriter _projectWriter;
        private string _newConnectionString;
        public string NewConnectionString
        {
            get => _newConnectionString;
            set
            {
                _newConnectionString = value;
                OnPropertyChanged();
            }
        }
        private string _selectedLang;
        public string SelectedLang
        {
            get => _selectedLang;
            set
            {
                _selectedLang = value;
                _languageChanger.ChangeLanguage(_selectedLang);
                UserSettings.Default.LanguageCode = _selectedLang;
                UserSettings.Default.Save();
            }
        }

        /// <summary>
        /// Observable collection of projects from shared data.
        /// </summary>
        public ObservableCollection<ProjectsInfo>? ProjectList => _sharedDataService.ProjectList;

        /// <summary>
        /// Current active ViewModel in the navigation store.
        /// </summary>
        public ViewModelBase? CurrentViewModel => _navigationStore.CurrentViewModel;

        /// <summary>
        /// Command to open a project creation dialog.
        /// </summary>
        public ICommand OpenDialogCommand { get; }

        /// <summary>
        /// Command to open a selected project.
        /// </summary>
        public ICommand OpenProjectCommand { get; }

        public ICommand SetNewConnectionStringCommand { get; }
        public ICommand DeleteSelectedProjectsCommand { get; }
        /// <summary>
        /// Constructor for MainWindowViewModel.
        /// </summary>
        public MainWindowViewModel(
            NavigationStore navigationStore,
            DialogShowingService projectDialogCreator,
            INavigationService navigationService,
            SharedDataService sharedDataService,
            ITaskLoader taskLoader,
            LanguageChanger languageChanger,
            DbManager dbManager,
            IProjectWriter projectWriter)
        {
            _navigationStore = navigationStore;
            _creator = projectDialogCreator;
            _navigationService = navigationService;
            _sharedDataService = sharedDataService;
            _taskLoader = taskLoader;
            _languageChanger = languageChanger;
            _dbManager = dbManager;
            _projectWriter = projectWriter;

            _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;

            // Subscribe to property changes in SharedDataService to update ProjectList
            SubscribeToServiceProperty(_sharedDataService,
                nameof(_sharedDataService.ProjectList),
                nameof(ProjectList));

            DeleteSelectedProjectsCommand = new AsyncRelayCommand(DeleteSelectedProjects);
            SetNewConnectionStringCommand = new AsyncRelayCommand(SetNewConnectionString);
            OpenDialogCommand = new AsyncRelayCommand(ShowCreateProjectDialog);
            OpenProjectCommand = new AsyncRelayCommand<ProjectsInfo>(OpenProject, _ => true);
        }

        /// <summary>
        /// Opens the selected project and loads tasks async.
        /// </summary>
        public async Task SetNewConnectionString()
        {
          if (await _dbManager.SetConnection(NewConnectionString))
            {
                UserSettings.Default.ConnectionString = NewConnectionString;
                UserSettings.Default.Save();
            }
        }
        public async Task OpenProject(ProjectsInfo project)
        {
            _sharedDataService.TaskList = await _taskLoader.LoadTaskAsync(project.Id);
            _navigationService.NavigateTo<ProjectViewModel>(project);
        }

        /// <summary>
        /// Shows the dialog to create a new project.
        /// </summary>
        private async Task ShowCreateProjectDialog()
        {
            await _creator.ShowCreateDialogAsync<CreateProjectDialogView, DialogsViewModel>(this, null);
        }

        private async Task DeleteSelectedProjects()
        {
            var projectsToDelete = ProjectList?.Where(x => x.IsSelected).ToList();

            if (projectsToDelete != null && projectsToDelete.Any())
            {
                foreach (var project in projectsToDelete)
                {
                    await _projectWriter.DeleteAsync(project.Id);

                    ProjectList?.Remove(project);
                }
            }
        }

        /// <summary>
        /// Handles changes in the current ViewModel of the navigation store.
        /// </summary>
        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }

        /// <summary>
        /// Unsubscribes from event.
        /// </summary>
        public void Dispose()
        {
            _navigationStore.CurrentViewModelChanged -= OnCurrentViewModelChanged;
        }
    }
}
