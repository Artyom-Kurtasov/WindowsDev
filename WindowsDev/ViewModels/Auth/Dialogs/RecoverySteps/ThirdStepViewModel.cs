using WindowsDev.Domain.PasswordRecoveryModels;

namespace WindowsDev.ViewModels.Auth.Dialogs.RecoverySteps
{
    public class ThirdStepViewModel : ViewModelBase
    {
        private readonly PasswordRecoveryData _passwordRecoveryData;

        public ThirdStepViewModel(PasswordRecoveryData passwordRecoveryData)
        {
            _passwordRecoveryData = passwordRecoveryData;
        }

        public string NewPassword
        {
            get => _passwordRecoveryData.NewPassword;
            set
            {
                if (_passwordRecoveryData.NewPassword == value)
                    return;

                _passwordRecoveryData.NewPassword = value;
                OnPropertyChanged(nameof(NewPassword));
            }
        }

        public string ConfirmPassword
        {
            get => _passwordRecoveryData.ConfirmPassword;
            set
            {
                if (_passwordRecoveryData.ConfirmPassword == value)
                    return;

                _passwordRecoveryData.ConfirmPassword = value;
                OnPropertyChanged(nameof(ConfirmPassword));
            }
        }
    }
}
