using WindowsDev.Factories.Interfaces;
using WindowsDev.NavigationManager;
using WindowsDev.ViewModels.Interfaces;
using WindowsDev.ViewModels.Main.Tabs;

namespace WindowsDev.ViewModels.Main
{
    public class MainWindowViewModel : ViewModelBase, IDisposable
    {
        private readonly IViewModelFactory _factory;
        private readonly NavigationStore _navigationStore;

        public MainWindowViewModel(NavigationStore navigationStore, IViewModelFactory factory)
        {
            _navigationStore = navigationStore;
            _factory = factory;

            _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;

            // NOTE: Initialize the default tab during startup since it is displayed immediately.
            _projects = _factory.Create<ProjectsViewModel>();

            if (_projects is IRefreshableViewModel refreshable)
            {
                _ = refreshable.RefreshAsync();
            }
        }

        public ViewModelBase? CurrentViewModel => _navigationStore.CurrentViewModel;

        private ProjectsViewModel? _projects;
        public ProjectsViewModel? Projects
        {
            get => _projects;
            set => _projects = value;
        }

        private SettingsViewModel? _settings;
        public SettingsViewModel? Settings
        {
            get => _settings;
            set
            {
                if (_settings == value)
                    return;
                _settings = value;
                OnPropertyChanged();
            }
        }

        private ProfileViewModel? _profile;
        public ProfileViewModel? Profile
        {
            get => _profile;
            set
            {
                if (_profile == value)
                    return;
                _profile = value;
                OnPropertyChanged();
            }
        }

        private int _selectedTabIndex;
        private bool disposedValue;

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                if (_selectedTabIndex == value)
                    return;
                _selectedTabIndex = value;
                OnPropertyChanged(nameof(SelectedTabIndex));

                LoadTab(_selectedTabIndex);
            }
        }

        private void LoadTab(int tabIndex)
        {
            if (tabIndex == 1)
            {
                if (_settings == null)
                {
                    Settings = _factory.Create<SettingsViewModel>();
                }
            }
            else if (tabIndex == 2)
            {
                if (_profile == null)
                {
                    Profile = _factory.Create<ProfileViewModel>();
                }
            }
        }

        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Projects = null;
                    Settings = null;
                    Profile = null;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
