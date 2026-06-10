using WindowsDev.Commands.NavigationManager;
using WindowsDev.ViewModels.Interfaces;
using WindowsDev.ViewModels.Main.Tabs;
using WindowsDev.Factories.Interfaces;

namespace WindowsDev.ViewModels.Main
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IViewModelFactory _factory;
        private readonly NavigationStore _navigationStore;

        private ProjectsViewModel _projects;
        private SettingsViewModel _settings;
        private ProfileViewModel _profile;

        public MainWindowViewModel(
            NavigationStore navigationStore,
            IViewModelFactory factory)
        {
            _navigationStore = navigationStore;
            _factory = factory;

            _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
        }

        public ViewModelBase? CurrentViewModel => _navigationStore.CurrentViewModel;

        public ProjectsViewModel Projects => _projects ??= _factory.Create<ProjectsViewModel>();
        public SettingsViewModel Settings => _settings ??= _factory.Create<SettingsViewModel>();
        public ProfileViewModel Profile => _profile ??= _factory.Create<ProfileViewModel>();

        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }
    }
}