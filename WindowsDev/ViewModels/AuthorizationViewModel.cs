using System.Windows.Input;
using WindowsDev.Businnes.Services;
using WindowsDev.Businnes.Services.ProjectService.Interfaces;
using WindowsDev.Commands.NavigationManager.Interfaces;
using WindowsDev.Infrastructure;

namespace WindowsDev.ViewModels
{
    public class AuthorizationViewModel : ViewModelBase
    {
        private readonly Authorization _authorization;
        private readonly INavigationService _navigationService;
        private readonly IProjectLoader _projectLoader;

        private SharedDataService _sharedDataService;

        private string _username;
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }
        public ICommand AuthorizeCommand { get; }
        public ICommand SwitchToRegViewCommand { get; }
        public AuthorizationViewModel(INavigationService navigationService, Authorization authorization, SharedDataService sharedDataService,
            IProjectLoader projectLoader)
        {
            _navigationService = navigationService;
            _authorization = authorization;
            _sharedDataService = sharedDataService;
            _projectLoader = projectLoader;

            SwitchToRegViewCommand = new RelayCommand(SwitchToRegView, CanSwitchToRegView);
            AuthorizeCommand = new AsyncRelayCommand(Authorize, CanAuthorize);
        }

        private void SwitchToRegView() => _navigationService.NavigateTo<RegistrationViewModel>();
        private bool CanSwitchToRegView() => true;
        private async Task Authorize()
        {
            if (_authorization.Authorize(_username, _password))
            {
                _navigationService.NavigateTo<MainWindowViewModel>();
                _sharedDataService.ProjectList = await _projectLoader.LoadProjectAsync();
            }
        }
        private bool CanAuthorize() => true;
    }
}
