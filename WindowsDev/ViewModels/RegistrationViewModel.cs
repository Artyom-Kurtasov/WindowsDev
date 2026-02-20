using System.Windows.Input;
using WindowsDev.Commands.NavigationManager.Interfaces;
using WindowsDev.Infrastructure;

namespace WindowsDev.ViewModels
{
    public class RegistrationViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;

        public ICommand SwitchToAuthView {  get; } //Command to switch main window datacontext to authorizationview content

        public RegistrationViewModel(INavigationService navigationStore)
        {
            _navigationService = navigationStore;

            SwitchToAuthView = new RelayCommand(ToAuthView, CanToAuthView);
        }

        private void ToAuthView() => _navigationService.NavigateTo<AuthorizationViewModel>();
        private bool CanToAuthView() => true;
    }
}
