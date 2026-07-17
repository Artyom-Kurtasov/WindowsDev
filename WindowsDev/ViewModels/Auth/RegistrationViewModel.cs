using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using WindowsDev.Business.Services.DebounceService;
using WindowsDev.Business.Services.Localization.Interfaces;
using WindowsDev.Business.Services.Registration.Interfaces;
using WindowsDev.Business.Services.Registration.Validation;
using WindowsDev.Domain;
using WindowsDev.Domain.DialogsMessages.Errors;
using WindowsDev.Domain.DialogsMessages.Informations;
using WindowsDev.Infrastructure;
using WindowsDev.Infrastructure.Logging;
using WindowsDev.NavigationManager.Interfaces;
using WindowsDev.ViewModels.Main;

namespace WindowsDev.ViewModels.Auth
{
    public class RegistrationViewModel : LocalizedViewModelBase
    {
        private readonly ILogger<RegistrationViewModel> _logger;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly INavigationService _navigationService;
        private readonly IRegistration _registration;
        private readonly IDebounceService _debounceService;

        private const int DebounceDelayMilliseconds = 500;

        public RegistrationViewModel(
            INavigationService navigationService,
            IRegistration registration,
            IDialogCoordinator dialogCoordinator,
            ILogger<RegistrationViewModel> logger,
            IDebounceService debounceService,
            ILanguageChanger languageChanger
        )
            : base(languageChanger)
        {
            _navigationService = navigationService;
            _registration = registration;
            _dialogCoordinator = dialogCoordinator;
            _logger = logger;
            _debounceService = debounceService;

            SignUpCommand = new AsyncRelayCommand(SignUpAsync);
            SwitchToAuthViewCommand = new AsyncRelayCommand(SwitchToAuthViewAsync);
        }

        public ICommand SignUpCommand { get; }
        public ICommand SwitchToAuthViewCommand { get; }

        private string _login = string.Empty;

        public string Login
        {
            get => _login;
            set
            {
                if (_login == value)
                    return;

                _login = value;

                OnPropertyChanged();

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
                if (_username == value)
                    return;

                _username = value;

                OnPropertyChanged();

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
                if (_password == value)
                    return;

                _password = value;

                OnPropertyChanged();

                UpdateState();
            }
        }

        private string _confirmPassword = string.Empty;

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                if (_confirmPassword == value)
                    return;

                _confirmPassword = value;

                OnPropertyChanged();

                UpdateState();
            }
        }

        private bool _isLoginAvailable;

        public bool IsLoginAvailable
        {
            get => _isLoginAvailable;
            private set
            {
                if (_isLoginAvailable == value)
                    return;

                _isLoginAvailable = value;

                OnPropertyChanged();

                UpdateState();
            }
        }

        private bool _isUsernameAvailable;

        public bool IsUsernameAvailable
        {
            get => _isUsernameAvailable;
            private set
            {
                if (_isUsernameAvailable == value)
                    return;

                _isUsernameAvailable = value;

                OnPropertyChanged();

                UpdateState();
            }
        }

        private string _errorMessage = string.Empty;

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (_errorMessage == value)
                    return;

                _errorMessage = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(HasError));
            }
        }

        public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

        private bool IsFormFilled()
        {
            return !string.IsNullOrWhiteSpace(Login)
                && !string.IsNullOrWhiteSpace(Username)
                && !string.IsNullOrWhiteSpace(Password);
        }

        private async Task SignUpAsync()
        {
            ErrorMessage = string.Empty;

            if (!CanSignUp())
            {
                ErrorMessage = Translate(AuthErrors.RegistrationFailed);
                return;
            }

            try
            {
                var result = await _registration.Register(Password, Login, Username);

                if (result.IsFailure)
                    return;

                await _dialogCoordinator.ShowMessageAsync(
                    this,
                    Translate(DialogTitles.Information),
                    $"{Translate(PasswordRecoveryInformations.RecoveryCodeMessage)}\n\n{result.Value}",
                    MessageDialogStyle.Affirmative
                );

                await _navigationService.NavigateTo<MainWindowViewModel>();
            }
            catch (Exception ex)
            {
                AuthLogs.RegistrationFailed(_logger, ex);

                await _dialogCoordinator.ShowMessageAsync(
                    this,
                    Translate(DialogTitles.Error),
                    Translate(CommonErrors.UnexpectedError),
                    MessageDialogStyle.Affirmative
                );
            }
        }

        private async Task SwitchToAuthViewAsync()
        {
            await _navigationService.NavigateTo<AuthorizationViewModel>();
        }

        private bool CanSignUp()
        {
            return IsFormFilled()
                && IsLoginAvailable
                && IsUsernameAvailable
                && Password == ConfirmPassword
                && PasswordValidator.IsValid(Password);
        }

        private async Task CheckLoginAvailabilityAsync()
        {
            await CheckFieldAvailabilityAsync(
                async () => IsLoginAvailable = await _registration.IsLoginAvailableAsync(Login),
                ex => AuthLogs.LoginAvailabilityCheckFailed(_logger, ex)
            );
        }

        private async Task CheckUsernameAvailabilityAsync()
        {
            await CheckFieldAvailabilityAsync(
                async () =>
                    IsUsernameAvailable = await _registration.IsUsernameAvailableAsync(Username),
                ex => AuthLogs.UsernameAvailabilityCheckFailed(_logger, ex)
            );
        }

        private async Task CheckFieldAvailabilityAsync(Func<Task> action, Action<Exception> log)
        {
            try
            {
                await _debounceService.DebounceAsync(
                    action,
                    TimeSpan.FromMilliseconds(DebounceDelayMilliseconds)
                );
            }
            catch (Exception ex)
            {
                log(ex);
            }
        }

        private void UpdateState()
        {
            ErrorMessage = string.Empty;
            ((AsyncRelayCommand)SignUpCommand).RaiseCanExecuteChanged();
        }
    }
}
