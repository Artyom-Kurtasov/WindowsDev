using WindowsDev.Business.Repositories.Interfaces;
using WindowsDev.Domain.PasswordRecoveryModels;

namespace WindowsDev.ViewModels.Auth.Dialogs.RecoverySteps
{
    public class FirstStepViewModel : ViewModelBase
    {
        private readonly PasswordRecoveryData _passwordRecoveryData;
        private readonly IUserRepository _userRepository;

        public FirstStepViewModel(PasswordRecoveryData passwordRecoveryData,
                                  IUserRepository userRepository)
        {
            _passwordRecoveryData = passwordRecoveryData;
            _userRepository = userRepository;
        }

        // Used to cancel previous debounced validation requests
        private CancellationTokenSource? _loginCts;

        public string Login
        {
            get => _passwordRecoveryData.Login;
            set
            {
                _passwordRecoveryData.Login = value;
                OnPropertyChanged(nameof(Login));

                _ = CheckUserAsync();
            }
        }

        public bool IsUserExist
        {
            get => _passwordRecoveryData.IsUserExist;
            set
            {
                _passwordRecoveryData.IsUserExist = value;
                OnPropertyChanged(nameof(IsUserExist));
            }
        }

        private async Task CheckUserAsync()
        {
            _loginCts?.Cancel();
            _loginCts = new CancellationTokenSource();

            try
            {
                await Task.Delay(500, _loginCts.Token);

                IsUserExist =
                    await _userRepository.ExistsByLoginAsync(Login);
            }
            catch (TaskCanceledException) { }
        }
    }
}
