using WindowsDev.Commands.NavigationManager;
using WindowsDev.Factories.Interfaces;
using WindowsDev.ViewModels.Main.Tabs;

namespace WindowsDev.ViewModels.Main
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IViewModelFactory _factory;
        private readonly NavigationStore _navigationStore;

        public MainWindowViewModel(NavigationStore navigationStore,
            IViewModelFactory factory)
        {
            _navigationStore = navigationStore;
            _factory = factory;

            _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
        }

        public ViewModelBase? CurrentViewModel => _navigationStore.CurrentViewModel;

        private ProjectsViewModel _projects;
        public ProjectsViewModel Projects => _projects ??= _factory.Create<ProjectsViewModel>();

        private SettingsViewModel _settings;
        public SettingsViewModel Settings => _settings ??= _factory.Create<SettingsViewModel>();

        private ProfileViewModel _profile;
        public ProfileViewModel Profile => _profile ??= _factory.Create<ProfileViewModel>();

        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }
    }
}