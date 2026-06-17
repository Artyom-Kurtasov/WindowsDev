using MahApps.Metro.Controls.Dialogs;
using System.Windows.Input;
using WindowsDev.Business.Services.Registration.Interfaces;
using WindowsDev.Business.Services.Registration.Validation;
using WindowsDev.Infrastructure;
using WindowsDev.NavigationManager.Interfaces;
using WindowsDev.ViewModels.Main;

namespace WindowsDev.ViewModels.Auth
{
    public class RegistrationViewModel : ViewModelBase
    {
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly INavigationService _navigationService;
        private readonly IRegistration _registration;
        private readonly UserFieldValidator _userFieldValidator;

        // Used to cancel previous debounced validation requests
        private CancellationTokenSource? _loginCts;
        private CancellationTokenSource? _usernameCts;

        public RegistrationViewModel(INavigationService navigationService,
            IRegistration registration,
            IDialogCoordinator dialogCoordinator,
            UserFieldValidator userFieldValidator)
        {
            _dialogCoordinator = dialogCoordinator;
            _navigationService = navigationService;
            _registration = registration;
            _userFieldValidator = userFieldValidator;

            SignUpCommand = new AsyncRelayCommand(SignUpAsync, CanSignUp);
            SwitchToAuthViewCommand = new RelayCommand(SwitchToAuthView);
        }

        public ICommand SignUpCommand { get; }
        public ICommand SwitchToAuthViewCommand { get; }

        private string _login = string.Empty;
        public string Login
        {
            get => _login;
            set
            {
                if (_login == value) return;

                _login = value;
                OnPropertyChanged(nameof(Login));

                _ = CheckLoginAvailabilityAsync();
                UpdateState();
            }
        }

        private string _username = string.Empty;
        public string Username
        {
            get => _username;
            set
            {
                if (_username == value) return;

                _username = value;
                OnPropertyChanged(nameof(Username));

                _ = CheckUsernameAvailabilityAsync();
                UpdateState();
            }
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set
            {
                if (_password == value) return;

                _password = value;
                OnPropertyChanged(nameof(Password));

                OnPasswordChanged();
            }
        }

        private string _confirmPassword = string.Empty;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                if (_confirmPassword == value) return;

                _confirmPassword = value;
                OnPropertyChanged(nameof(ConfirmPassword));

                UpdateState();
            }
        }

        private bool _isLoginAvailable;
        public bool IsLoginAvailable
        {
            get => _isLoginAvailable;
            internal set
            {
                if (_isLoginAvailable == value) return;
                _isLoginAvailable = value;
                OnPropertyChanged(nameof(IsLoginAvailable));
            }
        }

        private bool _isUsernameAvailable;
        public bool IsUsernameAvailable
        {
            get => _isUsernameAvailable;
            internal set
            {
                if (_isUsernameAvailable == value) return;
                _isUsernameAvailable = value;
                OnPropertyChanged(nameof(IsUsernameAvailable));
            }
        }

        private bool _isRegistrationFailed;
        public bool IsRegistrationFailed
        {
            get => _isRegistrationFailed;
            set
            {
                _isRegistrationFailed = value;
                OnPropertyChanged(nameof(IsRegistrationFailed));
            }
        }

        private bool _isFormFilled =>
            !string.IsNullOrWhiteSpace(Login) &&
            !string.IsNullOrWhiteSpace(Username) &&
            !string.IsNullOrWhiteSpace(Password);

        private void SwitchToAuthView()
        {
            _navigationService.NavigateTo<AuthorizationViewModel>();
        }

        private async Task SignUpAsync()
        {
            if (!IsLoginAvailable || !IsUsernameAvailable ||
                Password != ConfirmPassword || !PasswordValidator.IsValid(Password) || !_isFormFilled)
            {
                IsRegistrationFailed = true;
                return;
            }

            try
            {
                var recoveryCode = await _registration.Register(Password, Login, Username);
                IsRegistrationFailed = false;
                await _dialogCoordinator.ShowMessageAsync(this, Translate("Information_Title"), $"{Translate("RecoveryKeyMessage")}\n\n {recoveryCode}", MessageDialogStyle.Affirmative);
                await _navigationService.NavigateTo<MainWindowViewModel>();
            }
            catch (Exception ex)
            {
                IsRegistrationFailed = true;
                await _dialogCoordinator.ShowMessageAsync(this, Translate("Warning_Title"), Translate(ex.Message), MessageDialogStyle.Affirmative);
            }
        }

        private bool CanSignUp() => true;

        private async Task CheckLoginAvailabilityAsync()
        {
            _loginCts?.Cancel();
            _loginCts = new CancellationTokenSource();

            try
            {
                await Task.Delay(500, _loginCts.Token);

                IsLoginAvailable =
                    await _userFieldValidator.IsLoginAvailableAsync(Login);
            }
            catch (TaskCanceledException) { }
        }

        private async Task CheckUsernameAvailabilityAsync()
        {
            _usernameCts?.Cancel();
            _usernameCts = new CancellationTokenSource();

            try
            {
                await Task.Delay(500, _usernameCts.Token);

                IsUsernameAvailable =
                    await _userFieldValidator.IsUsernameAvailableAsync(Username);
            }
            catch (TaskCanceledException) { }
        }

        private void UpdateState()
        {
            _isRegistrationFailed = false;
            ((AsyncRelayCommand)SignUpCommand).RaiseCanExecuteChanged();
        }

        private void OnPasswordChanged()
        {
            UpdateState();
        }
    }
}
