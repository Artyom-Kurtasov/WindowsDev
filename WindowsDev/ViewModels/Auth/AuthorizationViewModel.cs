using Microsoft.Extensions.Logging;
using System.Windows.Input;
using WindowsDev.Business.Services.Authorization.Interfaces;
using WindowsDev.Dialogs.Interfaces;
using WindowsDev.Domain;
using WindowsDev.Domain.DialogsMessages.Errors;
using WindowsDev.Infrastructure;
using WindowsDev.Infrastructure.Logging;
using WindowsDev.NavigationManager.Interfaces;
using WindowsDev.ViewModels.Auth.Dialogs;
using WindowsDev.ViewModels.Main;
using WindowsDev.Views.Auth.Dialogs;

namespace WindowsDev.ViewModels.Auth
{
    public class AuthorizationViewModel : ViewModelBase
    {
        private readonly ILogger _logger;
        private readonly IDialogService _dialogService;
        private readonly IAuthorization _authorization;
        private readonly INavigationService _navigationService;

        public AuthorizationViewModel(INavigationService navigationService,
            IAuthorization authorization,
            IDialogService dialogService,
            ILogger logger)
        {
            _dialogService = dialogService;
            _navigationService = navigationService;
            _authorization = authorization;
            _logger = logger;

            SwitchToRegViewCommand = new AsyncRelayCommand(SwitchToRegViewAsync);
            AuthorizeCommand = new AsyncRelayCommand(AuthorizeAsync, CanAuthorize);
            PasswordRecoveryCommand = new AsyncRelayCommand(PasswordRecovery);
        }

        public ICommand PasswordRecoveryCommand { get; }
        public ICommand AuthorizeCommand { get; }
        public ICommand SwitchToRegViewCommand { get; }

        private string _login = string.Empty;
        public string Login
        {
            get => _login;
            set
            {
                _login = value;
                ErrorMessage = string.Empty;
                OnPropertyChanged(nameof(Login));
            }
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                ErrorMessage = string.Empty;
                OnPropertyChanged(nameof(Password));
            }
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
                OnPropertyChanged(nameof(HasError));
            }
        }

        public bool HasError =>
            !string.IsNullOrWhiteSpace(ErrorMessage);

        private async Task SwitchToRegViewAsync() =>
            await _navigationService.NavigateTo<RegistrationViewModel>();

        private bool CanAuthorize() => true;

        private async Task AuthorizeAsync()
        {
            ErrorMessage = string.Empty;

            try
            {

                var result = await _authorization.Authorize(Login, Password);

                if (result.IsFailure)
                {
                    ErrorMessage = Translate(result.Error);
                    return;
                }

                await _navigationService.NavigateTo<MainWindowViewModel>();
            }
            catch (Exception ex)
            {
                AuthLogs.AuthorizationFailed(_logger, ex);
                await _dialogService.ShowErrorDialogAsync(this,
                    Translate(DialogTitles.Error),
                    Translate(CommonErrors.UnexpectedError));
            }
        }

        private async Task PasswordRecovery() =>
            await _dialogService.ShowDialogAsync<
                RecoveryCodeDialogView,
                RecoveryCodeDialogViewModel>(this);
    }
}
