using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using System.Windows.Input;
using WindowsDev.Business.Services.Localization.Interfaces;
using WindowsDev.Business.Services.Profile.Interfaces;
using WindowsDev.Business.Services.UserManager.Interfaces;
using WindowsDev.Domain;
using WindowsDev.Domain.DialogsMessages.Errors;
using WindowsDev.Domain.DialogsMessages.Success;
using WindowsDev.Infrastructure;
using WindowsDev.Infrastructure.Logging;
using WindowsDev.NavigationManager.Interfaces;
using WindowsDev.ViewModels.Auth;

namespace WindowsDev.ViewModels.Main.Tabs
{
    public class ProfileViewModel : LocalizedViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly IProfileService _profileService;
        private readonly ICurrentUserService _userData;
        private readonly ILogger<ProfileViewModel> _logger;


        public ProfileViewModel(ICurrentUserService currentUserService,
            IProfileService profileService,
            IDialogCoordinator dialogCoordinator,
            INavigationService navigationService,
            ILogger<ProfileViewModel> logger,
            ILanguageChanger languageChanger) : base(languageChanger)
        {
            _userData = currentUserService;
            _profileService = profileService;
            _dialogCoordinator = dialogCoordinator;
            _navigationService = navigationService;
            _logger = logger;

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
                var result = await _profileService
                    .ChangeUsernameAsync(_userData.Username, Username);


                if (!result.IsSuccess)
                {
                    await _dialogCoordinator.ShowMessageAsync(
                        this,
                        Translate(DialogTitles.Warning),
                        Translate(result.Error),
                        MessageDialogStyle.Affirmative);

                    return;
                }


                await _dialogCoordinator.ShowMessageAsync(
                    this,
                    Translate(DialogTitles.Success),
                    Translate(ProfileSuccesses.UsernameChanged),
                    MessageDialogStyle.Affirmative);
            }
            catch (Exception ex)
            {
                ProfileLogs.UsernameChangeFailed(
                    _logger,
                    _userData.UserId,
                    ex);


                await _dialogCoordinator.ShowMessageAsync(
                    this,
                    Translate(DialogTitles.Error),
                    Translate(CommonErrors.UnexpectedError),
                    MessageDialogStyle.Affirmative);
            }
        }


        private async Task SaveNewPasswordAsync()
        {
            try
            {
                var result = await _profileService.ChangePasswordAsync(
                    CurrentPassword,
                    NewPassword,
                    ConfirmPassword);


                if (!result.IsSuccess)
                {
                    await _dialogCoordinator.ShowMessageAsync(
                        this,
                        Translate(DialogTitles.Warning),
                        Translate(result.Error),
                        MessageDialogStyle.Affirmative);

                    return;
                }


                await _dialogCoordinator.ShowMessageAsync(
                    this,
                    Translate(DialogTitles.Success),
                    $"{Translate(ProfileSuccesses.PasswordChanged)} {result.Value}",
                    MessageDialogStyle.Affirmative);
            }
            catch (Exception ex)
            {
                ProfileLogs.PasswordChangeFailed(
                    _logger,
                    _userData.UserId,
                    ex);


                await _dialogCoordinator.ShowMessageAsync(
                    this,
                    Translate(DialogTitles.Error),
                    Translate(CommonErrors.UnexpectedError),
                    MessageDialogStyle.Affirmative);
            }
        }


        private async Task LogoutAsync()
        {
            _userData.ClearUser();

            await _navigationService.NavigateTo<AuthorizationViewModel>();
        }


        private void SetUserData()
        {
            Id = _userData.UserId;
            Login = _userData.Login ?? string.Empty;
            Username = _userData.Username ?? string.Empty;
        }
    }
}