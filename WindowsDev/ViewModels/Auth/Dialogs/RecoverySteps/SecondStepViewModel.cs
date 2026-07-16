using WindowsDev.Business.Services.DebounceService;
using WindowsDev.Business.Services.PasswordManager.PasswordRecovery.Interfaces;
using WindowsDev.Domain.PasswordRecoveryModels;

namespace WindowsDev.ViewModels.Auth.Dialogs.RecoverySteps
{
    public class SecondStepViewModel : ViewModelBase
    {
        private readonly IPasswordRecoveryService _passwordRecoveryService;
        private readonly PasswordRecoveryData _passwordRecoveryData;
        private readonly IDebounceService _debounceService;

        private const int DebounceDelayMilliseconds = 500;


        public SecondStepViewModel(PasswordRecoveryData passwordRecoveryData,
            IPasswordRecoveryService passwordRecoveryService,
            IDebounceService debounceService)
        {
            _passwordRecoveryData = passwordRecoveryData;
            _passwordRecoveryService = passwordRecoveryService;
            _debounceService = debounceService;
        }


        public string RecoveryCode
        {
            get => _passwordRecoveryData.RecoveryCode;
            set
            {
                if (_passwordRecoveryData.RecoveryCode == value)
                    return;

                _passwordRecoveryData.RecoveryCode = value;

                OnPropertyChanged();

                _ = CheckRecoveryCodeAsync();
            }
        }


        public bool IsRecoveryCodeCorrect
        {
            get => _passwordRecoveryData.IsRecoveryCodeCorrect;
            private set
            {
                if (_passwordRecoveryData.IsRecoveryCodeCorrect == value)
                    return;

                _passwordRecoveryData.IsRecoveryCodeCorrect = value;

                OnPropertyChanged();
            }
        }


        private async Task CheckRecoveryCodeAsync()
        {
            await _debounceService.DebounceAsync(
                async () =>
                {
                    if (!int.TryParse(RecoveryCode, out int code))
                    {
                        IsRecoveryCodeCorrect = false;
                        return;
                    }

                    var result = await _passwordRecoveryService
                        .IsRecoverCodeCorrectAsync(code, _passwordRecoveryData.Login);

                    IsRecoveryCodeCorrect = result.IsSuccess;
                },
                TimeSpan.FromMilliseconds(DebounceDelayMilliseconds));
        }
    }
}