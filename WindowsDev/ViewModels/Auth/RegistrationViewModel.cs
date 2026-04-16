using System.Windows.Input;
using WindowsDev.Business.Services.Registration;
using WindowsDev.Business.Services.Registration.Validation;
using WindowsDev.Commands.NavigationManager.Interfaces;
using WindowsDev.Infrastructure;
using WindowsDev.ViewModels.Main;

namespace WindowsDev.ViewModels.Auth
{
    /// <summary>
    /// ViewModel for user registration.
    /// Handles field validation, availability checks, and navigation.
    /// </summary>
    public class RegistrationViewModel : ViewModelBase
    {
        private readonly PasswordValidator _passwordValidator;
        private readonly INavigationService _navigationService;
        private readonly Registration _registration;
        private readonly UserFieldValidator _userFieldValidator;

        public bool HasMinimumLength => _passwordValidator.HasMinimumLength(Password);
        public bool HasNumber => _passwordValidator.HasNumber(Password);
        public bool HasUppercase => _passwordValidator.HasUppercase(Password);
        public bool HasSymbol => _passwordValidator.HasSymbol(Password); 

        private string _login;
        public string Login
        {
            get => _login;
            set
            {
                _login = value;
                OnPropertyChanged(nameof(Login));
                _ = CheckLoginAvailabilityAsync();
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
                _ = CheckUsernameAvailabilityAsync();
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
                OnPropertyChanged(nameof(HasMinimumLength));
                OnPropertyChanged(nameof(HasNumber));
                OnPropertyChanged(nameof(HasUppercase));
                OnPropertyChanged(nameof(HasSymbol));
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

        private bool _isLoginAvailable;
        public bool IsLoginAvailable
        {
            get => _isLoginAvailable;
            set
            {
                _isLoginAvailable = value;
                OnPropertyChanged(nameof(IsLoginAvailable));
            }
        }

        private bool _isUsernameAvailable;
        public bool IsUsernameAvailable
        {
            get => _isUsernameAvailable;
            set
            {
                _isUsernameAvailable = value;
                OnPropertyChanged(nameof(IsUsernameAvailable));
            }
        }

        public ICommand SignUpCommand { get; }
        public ICommand SwitchToAuthViewCommand { get; }


        /// <summary>
        /// Constructor for RegistrationViewModel.
        /// </summary>
        public RegistrationViewModel(
            INavigationService navigationService,
            Registration registration,
            UserFieldValidator userFieldValidator,
            PasswordValidator passwordValidator)
        {
            _navigationService = navigationService;
            _registration = registration;
            _userFieldValidator = userFieldValidator;
            _passwordValidator = passwordValidator;

            SwitchToAuthViewCommand = new RelayCommand(SwitchToAuthView, CanSwitchToAuthView);
            SignUpCommand = new AsyncRelayCommand(SignUp, CanSignUp);
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
                await _navigationService.NavigateTo<MainWindowViewModel>();
        }

        /// <summary>
        /// Determines if registration can be executed (all fields must be filled).
        /// </summary>
        private bool CanSignUp() =>
            IsLoginAvailable && IsUsernameAvailable &&
            Password == ConfirmPassword &&
            HasMinimumLength && HasNumber &&
            HasSymbol && HasUppercase &&
            !string.IsNullOrEmpty(Password) &&
            !string.IsNullOrEmpty(Login) &&
            !string.IsNullOrEmpty(Username);

        /// <summary>
        /// Always allow switching to authorization view.
        /// </summary>
        private bool CanSwitchToAuthView() => true;

        /// <summary>
        /// Checks if the login is already taken asynchronously with debounce.
        /// </summary>
        private async Task CheckLoginAvailabilityAsync()
        {
            IsLoginAvailable = await _userFieldValidator.IsLoginAvailableAsync(Login);
            ((AsyncRelayCommand)SignUpCommand).RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Checks if the username is already taken async with debounce.
        /// </summary>
        private async Task CheckUsernameAvailabilityAsync()
        {
            IsUsernameAvailable = await _userFieldValidator.IsUsernameAvailableAsync(Username);
            ((AsyncRelayCommand)SignUpCommand).RaiseCanExecuteChanged();
        }
    }
}

