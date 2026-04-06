using System.Collections.ObjectModel;
using System.Windows.Input;
using WindowsDev.Businnes.Services;
using WindowsDev.Businnes.Services.TaskService.Interfaces;
using WindowsDev.Commands.NavigationManager;
using WindowsDev.Commands.NavigationManager.Interfaces;
using WindowsDev.Domain.UsersAuthInfo;
using WindowsDev.Infrastructure;
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
        private readonly INavigationService _navigationService;
        private readonly ITaskLoader _taskLoader;

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

        /// <summary>
        /// Constructor for MainWindowViewModel.
        /// </summary>
        public MainWindowViewModel(
            NavigationStore navigationStore,
            DialogShowingService projectDialogCreator,
            INavigationService navigationService,
            SharedDataService sharedDataService,
            ITaskLoader taskLoader)
        {
            _navigationStore = navigationStore;
            _creator = projectDialogCreator;
            _navigationService = navigationService;
            _sharedDataService = sharedDataService;
            _taskLoader = taskLoader;

            _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;

            // Subscribe to property changes in SharedDataService to update ProjectList
            SubscribeToServiceProperty(_sharedDataService,
                nameof(_sharedDataService.ProjectList),
                nameof(ProjectList));

            OpenDialogCommand = new AsyncRelayCommand(ShowCreateProjectDialog);
            OpenProjectCommand = new AsyncRelayCommand<ProjectsInfo>(OpenProject, _ => true);
        }

        /// <summary>
        /// Opens the selected project and loads tasks async.
        /// </summary>
        public async Task OpenProject(ProjectsInfo project)
        {
            _sharedDataService.TaskList = await _taskLoader.LoadTaskAsync();
            _navigationService.NavigateTo<ProjectViewModel>(project);
        }

        /// <summary>
        /// Shows the dialog to create a new project.
        /// </summary>
        private async Task ShowCreateProjectDialog()
        {
            await _creator.ShowCreateDialogAsync<CreateProjectDialogView, DialogsViewModel>(this, null);
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