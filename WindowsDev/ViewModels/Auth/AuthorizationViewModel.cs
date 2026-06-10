using MahApps.Metro.Controls.Dialogs;
using System.Windows.Input;
using WindowsDev.Business.Services.Authorization.Interfaces;
using WindowsDev.Business.Services.ProjectService.Interfaces;
using WindowsDev.Commands.NavigationManager.Interfaces;
using WindowsDev.Dialogs.Interfaces;
using WindowsDev.Infrastructure;
using WindowsDev.ViewModels.Auth.Dialogs;
using WindowsDev.ViewModels.Main;
using WindowsDev.Views.Auth.Dialogs;

namespace WindowsDev.ViewModels.Auth
{
    public class AuthorizationViewModel : ViewModelBase, IProjectDialogCreator
    {
        private readonly IDialogService _dialogService;
        private readonly IAuthorization _authorization;
        private readonly INavigationService _navigationService;

        public AuthorizationViewModel(INavigationService navigationService,
                                      IAuthorization authorization,
                                      IDialogService dialogService)
        {
            _dialogService = dialogService;
            _navigationService = navigationService;
            _authorization = authorization;

            SwitchToRegViewCommand = new AsyncRelayCommand(SwitchToRegViewAsync);
            AuthorizeCommand = new AsyncRelayCommand(AuthorizeAsync, CanAuthorize);
            PasswordRecoveryCommand = new AsyncRelayCommand(PasswordRecovery);
        }

        // Commands
        public ICommand PasswordRecoveryCommand { get; }
        public ICommand AuthorizeCommand { get; }
        public ICommand SwitchToRegViewCommand { get; }

        // Inputs
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

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        // State
        private bool _isLoginFailed;

        public event Func<Task>? CloseRequested;
        public event Func<Task>? Completed;

        public bool IsLoginFailed
        {
            get => _isLoginFailed;
            set
            {
                _isLoginFailed = value;
                OnPropertyChanged(nameof(IsLoginFailed));
            }
        }

        // Commands logic
        private async Task SwitchToRegViewAsync() =>
            await _navigationService.NavigateTo<RegistrationViewModel>();

        private bool CanAuthorize() => true;

        private async Task AuthorizeAsync()
        {

            var success = await _authorization.Authorize(_login, _password);

            IsLoginFailed = !success;

            if (success)
                await _navigationService.NavigateTo<MainWindowViewModel>();
            
        }

        private async Task PasswordRecovery()
        {
            await _dialogService.ShowTaskDialogAsync<RecoveryCodeDialogView, RecoveryCodeDialogViewModel>(this);
        }
    }
}