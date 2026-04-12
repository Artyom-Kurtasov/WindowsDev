using WindowsDev.Commands.NavigationManager;
using WindowsDev.ViewModels.Interfaces;
using WindowsDev.ViewModels.Main.Tabs;

namespace WindowsDev.ViewModels.Main
{
    /// <summary>
    /// ViewModel for the main window, handles navigation, project list, and dialogs.
    /// Implements IDisposable to unsubscribe from events.
    /// </summary>
    public class MainWindowViewModel : ViewModelBase, IDisposable, IInitializableAsync
    {
        private readonly NavigationStore _navigationStore;

        /// <summary>
        /// Current active ViewModel in the navigation store.
        /// </summary>
        public ViewModelBase? CurrentViewModel => _navigationStore.CurrentViewModel;

        public ProjectsViewModel Projects { get; }
        public SettingsViewModel Settings { get; }

        /// <summary>
        /// Initializes MainWindowViewModel with required services and commands.
        /// </summary>
        public MainWindowViewModel(NavigationStore navigationStore, ProjectsViewModel projectsViewModel,
            SettingsViewModel settingsViewModel)
        {
            _navigationStore = navigationStore;

            Projects = projectsViewModel;
            Settings = settingsViewModel;

            _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
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

        public async Task InitializationAsync(params object[] parameters)
        {
            await Projects.InitializationAsync();
        }
    }
}

