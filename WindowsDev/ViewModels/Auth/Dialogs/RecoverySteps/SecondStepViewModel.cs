using WindowsDev.Business.Services.PasswordManager.PasswordRecovery;
using WindowsDev.Domain.PasswordRecoveryModels;

namespace WindowsDev.ViewModels.Auth.Dialogs.RecoverySteps
{
    public class SecondStepViewModel : ViewModelBase
    {
        private readonly IPasswordRecoveryService _passwordRecoveryService;
        private readonly PasswordRecoveryData _passwordRecoveryData;

        public SecondStepViewModel(PasswordRecoveryData passwordRecoveryData,
                                   IPasswordRecoveryService passwordRecoveryService)
        {
            _passwordRecoveryData = passwordRecoveryData;
            _passwordRecoveryService = passwordRecoveryService;
        }

        // Used to cancel previous debounced validation requests
        private CancellationTokenSource? _recoveryCodeCts;

        public string RecoveryCode
        {
            get => _passwordRecoveryData.RecoveryCode;
            set
            {
                _passwordRecoveryData.RecoveryCode = value;
                OnPropertyChanged(nameof(RecoveryCode));

                _ = CheckRecoveryCodeAsync();
            }
        }

        public bool IsRecoveryCodeCorrect
        {
            get => _passwordRecoveryData.IsRecoveryCodeCorrect;
            set
            {
                _passwordRecoveryData.IsRecoveryCodeCorrect = value;
                OnPropertyChanged(nameof(IsRecoveryCodeCorrect));
            }
        }

        private async Task CheckRecoveryCodeAsync()
        {
            _recoveryCodeCts?.Cancel();
            _recoveryCodeCts = new CancellationTokenSource();

            try
            {
                await Task.Delay(500, _recoveryCodeCts.Token);

                if (int.TryParse(RecoveryCode, out int code))
                {
                    IsRecoveryCodeCorrect =
                        await _passwordRecoveryService.IsRecoverCodeCorrect(code, _passwordRecoveryData.Login);
                }
            }
            catch (TaskCanceledException) { }
        }
    }
}
