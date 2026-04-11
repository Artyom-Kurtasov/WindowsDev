using System.Windows.Input;
using WindowsDev.Business.Services.Registration;
using WindowsDev.Business.Services.Registration.Validation;
using WindowsDev.Commands.NavigationManager.Interfaces;
using WindowsDev.Infrastructure;

namespace WindowsDev.ViewModels
{
    /// <summary>
    /// ViewModel for user registration.
    /// Handles field validation, availability checks, and navigation.
    /// </summary>
    public class RegistrationViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly Registration _registration;
        private readonly UserFieldValidator _userFieldValidator;

        private CancellationTokenSource _loginCts;
        private CancellationTokenSource _usernameCts;

        private string _login;
        public string Login
        {
            get => _login;
            set
            {
                _login = value;
                OnPropertyChanged(nameof(Login));
                CheckLoginAvailabilityAsyncCommand.Execute(null);
                ((AsyncRelayCommand)SignUpCommand).RaiseCanExecuteChanged();
            }
        }

        private string _username;
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
                CheckUsernameAvailabilityAsyncCommand.Execute(null);
                ((AsyncRelayCommand)SignUpCommand).RaiseCanExecuteChanged();
            }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
                ((AsyncRelayCommand)SignUpCommand).RaiseCanExecuteChanged();
            }
        }

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                OnPropertyChanged(nameof(ConfirmPassword));
                ((AsyncRelayCommand)SignUpCommand).RaiseCanExecuteChanged();
            }
        }

        private string _usernameValidationMessage;
        public string UsernameValidationMessage
        {
            get => _usernameValidationMessage;
            set
            {
                _usernameValidationMessage = value;
                OnPropertyChanged();
                ((AsyncRelayCommand)SignUpCommand).RaiseCanExecuteChanged();
            }
        }

        private string _loginValidationMessage;
        public string LoginValidationMessage
        {
            get => _loginValidationMessage;
            set
            {
                _loginValidationMessage = value;
                OnPropertyChanged();
                ((AsyncRelayCommand)SignUpCommand).RaiseCanExecuteChanged();
            }
        }

        public ICommand CheckLoginAvailabilityAsyncCommand { get; }
        public ICommand CheckUsernameAvailabilityAsyncCommand { get; }
        public ICommand SignUpCommand { get; }
        public ICommand SwitchToAuthViewCommand { get; }


        /// <summary>
        /// Constructor for RegistrationViewModel.
        /// </summary>
        public RegistrationViewModel(
            INavigationService navigationService,
            Registration registration,
            UserFieldValidator userFieldValidator)
        {
            _navigationService = navigationService;
            _registration = registration;
            _userFieldValidator = userFieldValidator;

            SwitchToAuthViewCommand = new RelayCommand(SwitchToAuthView, CanSwitchToAuthView);
            SignUpCommand = new AsyncRelayCommand(SignUp, CanSignUp);

            CheckLoginAvailabilityAsyncCommand = new AsyncRelayCommand(CheckLoginAvailabilityAsync);
            CheckUsernameAvailabilityAsyncCommand = new AsyncRelayCommand(CheckUsernameAvailabilityAsync);
        }

        /// <summary>
        /// Navigate to the authorization view.
        /// </summary>
        private void SwitchToAuthView() => _navigationService.NavigateTo<AuthorizationViewModel>();

        /// <summary>
        /// Perform user registration if passwords match and required fields are filled.
        /// </summary>
        private async Task SignUp()
        {
            if (string.IsNullOrEmpty(Password) || Password != ConfirmPassword)
                return;

            if (_registration.Registrate(Password, Login, Username))
                _navigationService.NavigateTo<MainWindowViewModel>();
        }

        /// <summary>
        /// Determines if registration can be executed (all fields must be filled).
        /// </summary>
        private bool CanSignUp() =>
            !string.IsNullOrEmpty(Login) &&
            !string.IsNullOrEmpty(Username) &&
            !string.IsNullOrEmpty(Password) &&
            !string.IsNullOrEmpty(ConfirmPassword);

        /// <summary>
        /// Always allow switching to authorization view.
        /// </summary>
        private bool CanSwitchToAuthView() => true;

        /// <summary>
        /// Checks if the login is already taken asynchronously with debounce.
        /// </summary>
        private async Task CheckLoginAvailabilityAsync()
        {
            _loginCts?.Cancel();
            _loginCts = new CancellationTokenSource();
            var token = _loginCts.Token;

            await Task.Delay(500, token); // debounce

            if (token.IsCancellationRequested)
                return;

            LoginValidationMessage = await _userFieldValidator.IsLoginTakenAsync(Login);
        }

        /// <summary>
        /// Checks if the username is already taken async with debounce.
        /// </summary>
        private async Task CheckUsernameAvailabilityAsync()
        {
            _usernameCts?.Cancel();
            _usernameCts = new CancellationTokenSource();
            var token = _usernameCts.Token;

            await Task.Delay(500, token); // debounce

            if (token.IsCancellationRequested)
                return;

            UsernameValidationMessage = await _userFieldValidator.IsUsernameTakenAsync(Username);
        }
    }
}

