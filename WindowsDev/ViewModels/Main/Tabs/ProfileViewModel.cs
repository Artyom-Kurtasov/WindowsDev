using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using System.Windows.Input;
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
    public class ProfileViewModel : ViewModelBase
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
            ILogger<ProfileViewModel> logger)
        {
            _userData = currentUserService;
            _profileService = profileService;
            _dialogCoordinator = dialogCoordinator;
            _navigationService = navigationService;
            _logger = logger;

            SaveNewPasswordCommand = new AsyncRelayCommand(SaveNewPassword);
            SaveNewUsernameCommand = new AsyncRelayCommand(SaveNewUsername);
            LogoutCommand = new AsyncRelayCommand(LogoutAsync);

            SetUserData();
        }

        private void SetUserData()
        {
            Id = _userData.UserId;
            Login = _userData.Login ?? string.Empty;
            Username = _userData.Username ?? string.Empty;
        }

        private int _id;
        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged(nameof(Id));
            }
        }

        private string _login = string.Empty;
        public string Login
        {
            get => _login;
            set
            {
                _login = value;
                OnPropertyChanged(nameof(Login));
            }
        }

        private string _username = string.Empty;
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        private string _currentPassword = string.Empty;
        public string CurrentPassword
        {
            get => _currentPassword;
            set
            {
                _currentPassword = value;
                OnPropertyChanged(nameof(CurrentPassword));
            }
        }

        private string _newPassword = string.Empty;
        public string NewPassword
        {
            get => _newPassword;
            set
            {
                _newPassword = value;
                OnPropertyChanged(nameof(NewPassword));
            }
        }

        private string _confirmPassword = string.Empty;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                OnPropertyChanged(nameof(ConfirmPassword));
            }
        }

        public ICommand SaveNewUsernameCommand { get; }
        public ICommand SaveNewPasswordCommand { get; }
        public ICommand LogoutCommand { get; }

        // Updates username with validation through profile service
        private async Task SaveNewUsername()
        {
            try
            {
                var result = await _profileService.ChangeUsernameAsync(_userData.Username, Username);
                if (result.IsSuccess)
                {
                    await _dialogCoordinator.ShowMessageAsync(this,
                        Translate(DialogTitles.Success),
                        Translate(ProfileSuccesses.UsernameChanged),
                        MessageDialogStyle.Affirmative);
                }
                else
                {
                    await _dialogCoordinator.ShowMessageAsync(this,
                        Translate(DialogTitles.Warning),
                        Translate(result.Error),
                        MessageDialogStyle.Affirmative);
                }
            }
            catch (Exception ex)
            {
                ProfileLogs.UsernameChangeFailed(_logger, _userData.UserId, ex);
                await _dialogCoordinator.ShowMessageAsync(this,
                    Translate(DialogTitles.Error),
                    Translate(CommonErrors.UnexpectedError),
                    MessageDialogStyle.Affirmative);
            }
        }

        // Updates password with validation through profile service
        private async Task SaveNewPassword()
        {
            try
            {
                var result = await _profileService.ChangePasswordAsync(CurrentPassword, NewPassword, ConfirmPassword);
                if (result.IsSuccess)
                {
                    await _dialogCoordinator.ShowMessageAsync(this,
                        Translate(DialogTitles.Success),
                        Translate(ProfileSuccesses.PasswordChanged),
                        MessageDialogStyle.Affirmative);
                }
                else
                {
                    await _dialogCoordinator.ShowMessageAsync(this,
                        Translate(DialogTitles.Warning),
                        Translate(result.Error),
                        MessageDialogStyle.Affirmative);
                }
            }
            catch (Exception ex)
            {
                ProfileLogs.PasswordChangeFailed(_logger, _userData.UserId, ex);
                await _dialogCoordinator.ShowMessageAsync(this,
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
    }
}
