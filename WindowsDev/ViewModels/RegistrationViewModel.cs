using System.Windows.Input;
using WindowsDev.Businnes.Services.Registration;
using WindowsDev.Businnes.Services.Registration.Validation;
using WindowsDev.Commands.NavigationManager.Interfaces;
using WindowsDev.Infrastructure;

namespace WindowsDev.ViewModels
{
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
                ((RelayCommand)SignUpCommand).RaiseCanExecuteChanged();
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
                ((RelayCommand)SignUpCommand).RaiseCanExecuteChanged();
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
                ((RelayCommand)SignUpCommand).RaiseCanExecuteChanged();
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
                ((RelayCommand)SignUpCommand).RaiseCanExecuteChanged();
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
                ((RelayCommand)SignUpCommand).RaiseCanExecuteChanged();
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
                ((RelayCommand)SignUpCommand).RaiseCanExecuteChanged();
            }
        }

        public ICommand CheckLoginAvailabilityAsyncCommand { get; }
        public ICommand CheckUsernameAvailabilityAsyncCommand { get; }
        public ICommand SignUpCommand { get; }
        public ICommand SwitchToAuthViewCommand { get; }

        public RegistrationViewModel(INavigationService navigationStore,
                                     Registration registration,
                                     UserFieldValidator userFieldValidator)
        {
            _navigationService = navigationStore;
            _registration = registration;
            _userFieldValidator = userFieldValidator;

            SwitchToAuthViewCommand = new RelayCommand(SwitchToAuthView, CanSwitchToAuthView);
            SignUpCommand = new RelayCommand(SignUp, CanSignUp);

            CheckLoginAvailabilityAsyncCommand = new AsyncRelayCommand(CheckLoginAvailabilityAsync);
            CheckUsernameAvailabilityAsyncCommand = new AsyncRelayCommand(CheckUsernameAvailabilityAsync);
        }

        private void SwitchToAuthView() => _navigationService.NavigateTo<AuthorizationViewModel>();

        private async void SignUp()
        {
            if (string.IsNullOrEmpty(Password) || Password != ConfirmPassword)
            {
                return;
            }

            if (_registration.Registrate(Password, Login, Username))
            {
                _navigationService.NavigateTo<MainWindowViewModel>();
            }
        }

        private bool CanSignUp() =>
            !string.IsNullOrEmpty(Login) &&
            !string.IsNullOrEmpty(Username) &&
            !string.IsNullOrEmpty(Password) &&
            !string.IsNullOrEmpty(ConfirmPassword);

        private bool CanSwitchToAuthView() => true;

        private async Task CheckLoginAvailabilityAsync()
        {
            _loginCts?.Cancel();
            _loginCts = new CancellationTokenSource();

            var token = _loginCts.Token;

            await Task.Delay(500, token);

            if (token.IsCancellationRequested)
            {
                return;
            }

            LoginValidationMessage = await _userFieldValidator.IsLoginTakenAsync(Login);
        }

        private async Task CheckUsernameAvailabilityAsync()
        {
            _usernameCts?.Cancel();
            _usernameCts = new CancellationTokenSource();

            var token = _usernameCts.Token;

            await Task.Delay(500, token);

            if (token.IsCancellationRequested)
            {
                return;
            }

            UsernameValidationMessage = await _userFieldValidator.IsUsernameTakenAsync(Username);
        }
    }
}