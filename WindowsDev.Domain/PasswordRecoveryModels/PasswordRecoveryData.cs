using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WindowsDev.Domain.PasswordRecoveryModels
{
    public class PasswordRecoveryData : INotifyPropertyChanged
    {
        private string? _login;
        public string? Login
        {
            get => _login;
            set
            {
                _login = value;
                OnPropertyChanged();
            }
        }

        private string? _recoveryCode;
        public string? RecoveryCode
        {
            get => _recoveryCode;
            set
            {
                _recoveryCode = value;
                OnPropertyChanged();
            }
        }

        private string? _newPassword;
        public string? NewPassword
        {
            get => _newPassword;
            set
            {
                _newPassword = value;
                OnPropertyChanged();
            }
        }

        private string? _confirmPassword;
        public string? ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                OnPropertyChanged();
            }
        }

        private bool _isUserExist;
        public bool IsUserExist
        {
            get => _isUserExist;
            set
            {
                _isUserExist = value;
                OnPropertyChanged();
            }
        }

        private bool _isRecoveryCodeCorrect;
        public bool IsRecoveryCodeCorrect
        {
            get => _isRecoveryCodeCorrect;
            set
            {
                _isRecoveryCodeCorrect = value;
                OnPropertyChanged(nameof(IsRecoveryCodeCorrect));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}