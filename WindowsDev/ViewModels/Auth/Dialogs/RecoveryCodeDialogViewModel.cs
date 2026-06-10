using MahApps.Metro.Controls.Dialogs;
using System.Windows.Input;
using WindowsDev.Business.Services.PasswordManager.PasswordRecovery;
using WindowsDev.Business.Services.ProjectService.Interfaces;
using WindowsDev.Infrastructure;

namespace WindowsDev.ViewModels.Auth.Dialogs
{
    public class RecoveryCodeDialogViewModel : ViewModelBase, IProjectDialogCreator
    {
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly IPasswordRecoveryService _passwordRecoveryService;
        public RecoveryCodeDialogViewModel(IDialogCoordinator dialogCoordinator,
                                           IPasswordRecoveryService passwordRecoveryService)
        {
            _dialogCoordinator = dialogCoordinator;
            _passwordRecoveryService = passwordRecoveryService;

            CloseDialogCommand = new AsyncRelayCommand(CloseDialogAsync);
            ConfirmCommand = new AsyncRelayCommand(ConfirmCodeAsync);
        }

        private string _recoveryCode;

        public event Func<Task>? CloseRequested;
        public event Func<Task>? Completed;

        public string RecoveryCode
        {
            get => _recoveryCode;
            set
            {
                _recoveryCode = value;
                OnPropertyChanged(nameof(RecoveryCode));
            }
        }

        private string _login;
        public string Login
        {
            get => _login;
            set
            {
                _login = value;
                OnPropertyChanged(nameof(Login));
            }
        }

        public ICommand CloseDialogCommand { get; }
        public ICommand ConfirmCommand { get; }

        private async Task CloseDialogAsync()
        {
            var dialog = await _dialogCoordinator.GetCurrentDialogAsync<BaseMetroDialog>(this);
            await _dialogCoordinator.HideMetroDialogAsync(this, dialog);
        }

        private async Task ConfirmCodeAsync()
        {
            if (int.TryParse(RecoveryCode, out int code))
            {
               if (await _passwordRecoveryService.IsRecoverCodeCorrect(code, Login))
                {
                    CloseRequested?.Invoke();
                }
            }
        }
    }
}
