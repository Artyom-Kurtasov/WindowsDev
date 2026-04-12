using System.Windows.Input;
using WindowsDev.Business.Services;
using WindowsDev.Commands.NavigationManager.Interfaces;
using WindowsDev.Infrastructure;
using WindowsDev.ViewModels.Main;

namespace WindowsDev.ViewModels.Auth
{
    /// <summary>
    /// ViewModel for user authorization.
    /// </summary>
    public class AuthorizationViewModel : ViewModelBase
    {
        private readonly Authorization _authorization;
        private readonly INavigationService _navigationService;

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
            Authorization authorization)
        {
            _navigationService = navigationService;
            _authorization = authorization;

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
                await _navigationService.NavigateTo<MainWindowViewModel>();
            }
        }

        /// <summary>
        /// Determines if authorization can be executed.
        /// </summary>
        private bool CanAuthorize() => true;
    }
}

