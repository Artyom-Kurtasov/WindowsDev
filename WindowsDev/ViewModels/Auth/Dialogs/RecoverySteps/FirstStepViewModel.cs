using Microsoft.Extensions.Logging;
using WindowsDev.Business.Services.DebounceService;
using WindowsDev.Business.Services.PasswordManager.PasswordRecovery.Interfaces;
using WindowsDev.Domain.PasswordRecoveryModels;
using WindowsDev.Infrastructure.Logging;

namespace WindowsDev.ViewModels.Auth.Dialogs.RecoverySteps
{
    public class FirstStepViewModel : ViewModelBase
    {
        private readonly PasswordRecoveryData _passwordRecoveryData;
        private readonly IPasswordRecoveryService _passwordRecoveryService;
        private readonly IDebounceService _debounceService;

        private const int DebounceDelayMilliseconds = 300;


        public FirstStepViewModel(PasswordRecoveryData passwordRecoveryData,
            IPasswordRecoveryService passwordRecoveryService,
            IDebounceService debounceService)
        {
            _passwordRecoveryData = passwordRecoveryData;
            _passwordRecoveryService = passwordRecoveryService;
            _debounceService = debounceService;
        }


        public string Login
        {
            get => _passwordRecoveryData.Login;
            set
            {
                if (_passwordRecoveryData.Login == value)
                    return;

                _passwordRecoveryData.Login = value;

                OnPropertyChanged();

                _ = CheckUserAsync();
            }
        }


        public bool IsUserExist
        {
            get => _passwordRecoveryData.IsUserExist;
            private set
            {
                if (_passwordRecoveryData.IsUserExist == value)
                    return;

                _passwordRecoveryData.IsUserExist = value;

                OnPropertyChanged();
            }
        }


        private async Task CheckUserAsync()
        {
            await _debounceService.DebounceAsync(
                async () =>
                {
                    IsUserExist = !string.IsNullOrWhiteSpace(Login) &&
                                  await _passwordRecoveryService
                                      .IsUserExistAsync(Login);
                },
                TimeSpan.FromMilliseconds(DebounceDelayMilliseconds));

        }
    }
}