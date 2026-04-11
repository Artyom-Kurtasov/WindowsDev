using System.Windows.Input;
using WindowsDev.Business.Services;
using WindowsDev.Business.Services.ProjectService.Interfaces;
using WindowsDev.Commands.NavigationManager.Interfaces;
using WindowsDev.Infrastructure;

namespace WindowsDev.ViewModels
{
    /// <summary>
    /// ViewModel for user authorization.
    /// </summary>
    public class AuthorizationViewModel : ViewModelBase
    {
        private readonly Authorization _authorization;
        private readonly INavigationService _navigationService;
        private readonly IProjectLoader _projectLoader;
        private readonly SharedDataService _sharedDataService;

        private string _username = string.Empty;
        /// <summary>
        /// Username input by the user.
        /// </summary>
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        private string _password = string.Empty;
        /// <summary>
        /// Password input by the user.
        /// </summary>
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        /// <summary>
        /// Command to perform authorization.
        /// </summary>
        public ICommand AuthorizeCommand { get; }

        /// <summary>
        /// Command to switch to registration view.
        /// </summary>
        public ICommand SwitchToRegViewCommand { get; }

        /// <summary>
        /// Constructor for AuthorizationViewModel.
        /// </summary>
        public AuthorizationViewModel(
            INavigationService navigationService,
            Authorization authorization,
            SharedDataService sharedDataService,
            IProjectLoader projectLoader)
        {
            _navigationService = navigationService;
            _authorization = authorization;
            _sharedDataService = sharedDataService;
            _projectLoader = projectLoader;

            SwitchToRegViewCommand = new RelayCommand(SwitchToRegView, CanSwitchToRegView);
            AuthorizeCommand = new AsyncRelayCommand(Authorize, CanAuthorize);
        }

        /// <summary>
        /// Navigate to the registration view.
        /// </summary>
        private void SwitchToRegView() => _navigationService.NavigateTo<RegistrationViewModel>();

        /// <summary>
        /// Determines if user can switch to registration view.
        /// </summary>
        private bool CanSwitchToRegView() => true;

        /// <summary>
        /// Perform authorization and load user's projects if successful.
        /// </summary>
        private async Task Authorize()
        {
            if (_authorization.Authorize(_username, _password))
            {
                _navigationService.NavigateTo<MainWindowViewModel>();
                _sharedDataService.ProjectList = await _projectLoader.LoadProjectAsync();
            }
        }

        /// <summary>
        /// Determines if authorization can be executed.
        /// </summary>
        private bool CanAuthorize() => true;
    }
}

