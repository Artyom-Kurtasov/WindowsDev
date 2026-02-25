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

        public string Username {  get; set; }
        public string Password {  get; set; }
        public string Email {  get; set; }

        public ICommand SwitchToAuthView {  get; } //Command to switch main window datacontext to authorizationview content

        public RegistrationViewModel(INavigationService navigationStore, Registration registration)
        {
            _navigationService = navigationStore;
            _registration = registration;

            SwitchToAuthView = new RelayCommand(ToAuthView, CanToAuthView);
        }

        private void ToAuthView() => _navigationService.NavigateTo<AuthorizationViewModel>();
        private bool CanToAuthView() => true;
    }
}
