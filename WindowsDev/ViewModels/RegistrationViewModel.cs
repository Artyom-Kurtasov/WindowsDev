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

        public required string Username {  get; set; }
        public  required string Password {  get; set; }
        public required string Email {  get; set; }

        public ICommand SignUp { get; }
        public ICommand SwitchToAuthView {  get; } //Command to switch main window datacontext to authorizationview content

        public RegistrationViewModel(INavigationService navigationStore, Registration registration)
        {
            _navigationService = navigationStore;
            _registration = registration;

            SwitchToAuthView = new RelayCommand(ToAuthView, CanToAuthView);
            SignUp = new RelayCommand(Sign, CanSign);
        }

        private void ToAuthView() => _navigationService.NavigateTo<AuthorizationViewModel>();
        private void Sign() => _registration.Adds(Password, Email, Username);
        private bool CanToAuthView() => true;
        private bool CanSign() => true;
    }
}
