using Microsoft.Extensions.Logging;
using System.Windows.Input;
using WindowsDev.Api.DTO.Request.AuthController;
using WindowsDev.ApiClients.AuthClient;
using WindowsDev.Application.Common.Utils.Localization;
using WindowsDev.Application.Identity;
using WindowsDev.Command;
using WindowsDev.Domain.Messages;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;
using WindowsDev.Infrastructure.Logging;
using WindowsDev.Services.Dialogs;
using WindowsDev.Services.Navigation;
using WindowsDev.ViewModels.Authorization.Dialogs;
using WindowsDev.ViewModels.Main;
using WindowsDev.ViewModels.Registration;
using WindowsDev.Views.Authorization.Dialogs;

namespace WindowsDev.ViewModels.Authorization;

internal class AuthorizationViewModel : LocalizedViewModelBase
{
    private readonly ISecureTokenStorage _tokenStorage;
    private readonly ILogger<AuthorizationViewModel> _logger;
    private readonly IDialogService _dialogService;
    private readonly IAuthApiClient _authApiClient;
    private readonly INavigationService _navigationService;

    public AuthorizationViewModel(
        INavigationService navigationService,
        IDialogService dialogService,
        IAuthApiClient authApiClient,
        ILogger<AuthorizationViewModel> logger,
        ILanguageChanger languageChanger,
        ISecureTokenStorage tokenStorage
    )
        : base(languageChanger)
    {
        _authApiClient = authApiClient;
        _navigationService = navigationService;
        _dialogService = dialogService;
        _logger = logger;
        _tokenStorage = tokenStorage;

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

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    private async Task SwitchToRegViewAsync()
    {
        await _navigationService.NavigateTo<RegistrationViewModel>();
    }

    private async Task AuthorizeAsync()
    {
        ErrorMessage = string.Empty;

        try
        {
            var request = new UserLoginRequest
            {
                Login = Login,
                Password = Password
            };

            var result = await _authApiClient.LoginAsync(request);

            if (result.IsFailure)
            {
                ErrorMessage = Translate(result.Error);
                return;
            }

            _tokenStorage.AccessToken = result.Value.JwtToken;
            await _navigationService.NavigateTo<MainWindowViewModel>();
        }
        catch (Exception ex)
        {
            AuthLogs.AuthorizationFailed(_logger, ex);

            await _dialogService.ShowErrorDialogAsync(
                Translate(DialogTitles.Error),
                Translate(CommonErrors.UnexpectedError)
            );
        }
    }

    private async Task PasswordRecoveryAsync()
    {
        await _dialogService.ShowDialogAsync<
            RecoveryCodeDialogView,
            RecoveryCodeDialogViewModel
        >(this);
    }

}