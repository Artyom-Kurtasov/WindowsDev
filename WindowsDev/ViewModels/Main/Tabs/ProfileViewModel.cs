using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using System.Windows.Input;
using WindowsDev.Api.DTO.Request.ProfilesController;
using WindowsDev.ApiClients.ProfileClient;
using WindowsDev.Application.Common.Utils.Localization;
using WindowsDev.Application.Identity;
using WindowsDev.Command;
using WindowsDev.Domain.Messages;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;
using WindowsDev.Domain.Messages.DialogsMessages.Success;
using WindowsDev.Infrastructure.Logging;
using WindowsDev.Services.Dialogs;
using WindowsDev.Services.Navigation;
using WindowsDev.ViewModels.Authorization;
using WindowsDev.ViewModels.Interfaces;

namespace WindowsDev.ViewModels.Main.Tabs;

internal class ProfileViewModel : LocalizedViewModelBase, IRefreshableViewModel
{
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly IProfileApiClient _profileApiClient;
    private readonly ILogger<ProfileViewModel> _logger;
    private readonly IUserSession _userSession;

    public ProfileViewModel(
        IProfileApiClient profileApiClient,
        IDialogService dialogService,
        INavigationService navigationService,
        ILogger<ProfileViewModel> logger,
        ILanguageChanger languageChanger,
        IUserSession userSession
    )
        : base(languageChanger)
    {
        _profileApiClient = profileApiClient;
        _dialogService = dialogService;
        _navigationService = navigationService;
        _logger = logger;
        _userSession = userSession;

        SaveNewPasswordCommand = new AsyncRelayCommand(SaveNewPasswordAsync);
        SaveNewUsernameCommand = new AsyncRelayCommand(SaveNewUsernameAsync);
        LogoutCommand = new AsyncRelayCommand(LogoutAsync);

        SetUserData();
    }

    public ICommand SaveNewUsernameCommand { get; }
    public ICommand SaveNewPasswordCommand { get; }
    public ICommand LogoutCommand { get; }

    private int _id;

    public int Id
    {
        get => _id;
        private set
        {
            if (_id == value)
                return;

            _id = value;
            OnPropertyChanged();
        }
    }

    private string _login = string.Empty;

    public string Login
    {
        get => _login;
        private set
        {
            if (_login == value)
                return;

            _login = value;
            OnPropertyChanged();
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
        }
    }

    private string _currentPassword = string.Empty;

    public string CurrentPassword
    {
        get => _currentPassword;
        set
        {
            if (_currentPassword == value)
                return;

            _currentPassword = value;
            OnPropertyChanged();
        }
    }

    private string _newPassword = string.Empty;

    public string NewPassword
    {
        get => _newPassword;
        set
        {
            if (_newPassword == value)
                return;

            _newPassword = value;
            OnPropertyChanged();
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
        }
    }

    private async Task SaveNewUsernameAsync()
    {
        try
        {
            var result = await _profileApiClient.ChangeUsernameAsync(
                new UsernameChangeRequest
                {
                    CurrentUsername = _userSession.Username,
                    NewUsername = Username,
                },
                CancellationToken.None
            );

            if (!result.IsSuccess)
            {
                await _dialogService.ShowMessageAsync(
                    Translate(DialogTitles.Warning),
                    Translate(result.Error),
                    MessageDialogStyle.Affirmative
                );

                return;
            }

            await _dialogService.ShowMessageAsync(
                Translate(DialogTitles.Success),
                Translate(ProfileSuccesses.UsernameChanged),
                MessageDialogStyle.Affirmative
            );
        }
        catch (Exception ex)
        {
            ProfileLogs.UsernameChangeFailed(_logger, _userSession.UserId, ex);

            await _dialogService.ShowMessageAsync(
                Translate(DialogTitles.Error),
                Translate(CommonErrors.UnexpectedError),
                MessageDialogStyle.Affirmative
            );
        }
    }

    private async Task SaveNewPasswordAsync()
    {
        try
        {
            var result = await _profileApiClient.ChangePasswordAsync(
                new ProfileChangePasswordRequest
                {
                    CurrentPassword = CurrentPassword,
                    NewPassword = NewPassword,
                    ConfirmPassword = ConfirmPassword,
                    Login = Login,
                },
                CancellationToken.None
            );

            if (!result.IsSuccess)
            {
                await _dialogService.ShowMessageAsync(
                    Translate(DialogTitles.Warning),
                    Translate(result.Error),
                    MessageDialogStyle.Affirmative
                );

                return;
            }

            await _dialogService.ShowMessageAsync(
                Translate(DialogTitles.Success),
                $"{Translate(ProfileSuccesses.PasswordChanged)} {result.Value.RecoveryCode}",
                MessageDialogStyle.Affirmative
            );
        }
        catch (Exception ex)
        {
            ProfileLogs.PasswordChangeFailed(_logger, _userSession.UserId, ex);

            await _dialogService.ShowMessageAsync(
                Translate(DialogTitles.Error),
                Translate(CommonErrors.UnexpectedError),
                MessageDialogStyle.Affirmative
            );
        }
    }

    private async Task LogoutAsync()
    {
        await _navigationService.NavigateTo<AuthorizationViewModel>();
    }

    private void SetUserData()
    {
        Id = _userSession.UserId;
        Login = _userSession.Login ?? string.Empty;
        Username = _userSession.Username ?? string.Empty;
    }

    public async Task RefreshAsync()
    {
        SetUserData();
    }
}