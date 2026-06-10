using MahApps.Metro.Controls.Dialogs;
using System.Windows.Input;
using WindowsDev.Business.Services.Profile.Interfaces;
using WindowsDev.Business.Services.UserManager.Interfaces;
using WindowsDev.Infrastructure;

namespace WindowsDev.ViewModels.Main.Tabs
{
    public class ProfileViewModel : ViewModelBase
    {
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly IProfileService _profileService;
        private readonly ICurrentUserService _userData;
        public ProfileViewModel(ICurrentUserService currentUserService,
                                IProfileService profileService,
                                IDialogCoordinator dialogCoordinator)
        {
            _userData = currentUserService;
            _profileService = profileService;
            _dialogCoordinator = dialogCoordinator;

            SaveNewPasswordCommand = new AsyncRelayCommand(SaveNewPassword);
            SaveNewUsernameCommand = new AsyncRelayCommand(SaveNewUsername);

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

        public async Task SaveNewUsername()
        {
            try
            {
                await _profileService.ChangeUsername(_userData.Username, Username);
            }
            catch (Exception ex)
            {
                await _dialogCoordinator.ShowMessageAsync(this, "Warning", ex.Message, MessageDialogStyle.Affirmative);
            }
        }

        public async Task SaveNewPassword()
        {
            try
            {
                await _profileService.ChangePassword(CurrentPassword, NewPassword, ConfirmPassword);
            }
            catch (Exception ex)
            {
                await _dialogCoordinator.ShowMessageAsync(this, "Warning", ex.Message, MessageDialogStyle.Affirmative);
            }
        }
    }
}
