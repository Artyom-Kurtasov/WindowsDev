using System.Windows.Input;
using WindowsDev.Commands;
using WindowsDev.Commands.NavigationManager.Interfaces;
using WindowsDev.Infrastructure;

namespace WindowsDev.ViewModels
{
    public class AuthorizationViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        public ICommand SwitchToRegView { get; } //Command to switch main window datacontext to registrationview content
        public AuthorizationViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

            SwitchToRegView = new RelayCommand(ToRegView, CanToRegView);
        }
        
        private void ToRegView() => _navigationService.NavigateTo<RegistrationViewModel>();
        private bool CanToRegView() => true;
    }
}
