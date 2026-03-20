using System.Windows.Input;
using WindowsDev.Businnes.Services;
using WindowsDev.Commands.NavigationManager.Interfaces;
using WindowsDev.Infrastructure;

namespace WindowsDev.ViewModels
{
    public class RegistrationViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly Registration _registration;

        private string _userName;
        public required string Username
        {
            get => _userName;
            set
            {
                _userName = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        private string _password;
        public required string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        public ICommand SignUpCommand { get; }
        public ICommand SwitchToAuthViewCommand { get; }

        public RegistrationViewModel(INavigationService navigationStore, Registration registration)
        {
            _navigationService = navigationStore;
            _registration = registration;

            SwitchToAuthViewCommand = new RelayCommand(ToAuthView, CanToAuthView);
            SignUpCommand = new RelayCommand(Sign, CanSign);
        }
        private void ToAuthView() => _navigationService.NavigateTo<AuthorizationViewModel>();
        private void Sign()
        {
            _registration.Registrate(_password, _userName);
            _navigationService.NavigateTo<MainWindowViewModel>();
        }
        private bool CanToAuthView() => true;
        private bool CanSign() => true;
    }
}
