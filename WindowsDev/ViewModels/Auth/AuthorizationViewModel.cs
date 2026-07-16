using Microsoft.Extensions.Logging;
using System.Windows.Input;
using WindowsDev.Business.Services.Authorization.Interfaces;
using WindowsDev.Business.Services.Localization.Interfaces;
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
    public class AuthorizationViewModel : LocalizedViewModelBase
    {
        private readonly ILogger<AuthorizationViewModel> _logger;
        private readonly IDialogService _dialogService;
        private readonly IAuthorization _authorization;
        private readonly INavigationService _navigationService;


        public AuthorizationViewModel(INavigationService navigationService,
            IAuthorization authorization,
            IDialogService dialogService,
            ILogger<AuthorizationViewModel> logger,
            ILanguageChanger languageChanger)
            : base(languageChanger)
        {
            _navigationService = navigationService;
            _authorization = authorization;
            _dialogService = dialogService;
            _logger = logger;

            SwitchToRegViewCommand = new AsyncRelayCommand(SwitchToRegViewAsync);
            AuthorizeCommand = new AsyncRelayCommand(AuthorizeAsync);
            PasswordRecoveryCommand = new AsyncRelayCommand(PasswordRecoveryAsync);
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
                if (_login == value)
                    return;

                _login = value;
                ErrorMessage = string.Empty;
                OnPropertyChanged();
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
                ErrorMessage = string.Empty;
                OnPropertyChanged();
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


        public bool HasError =>
            !string.IsNullOrWhiteSpace(ErrorMessage);


        private async Task SwitchToRegViewAsync()
        {
            await _navigationService.NavigateTo<RegistrationViewModel>();
        }


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


        private async Task PasswordRecoveryAsync()
        {
            await _dialogService.ShowDialogAsync<RecoveryCodeDialogView,
                RecoveryCodeDialogViewModel>(this);
        }
    }
}