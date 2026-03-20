using WindowsDev.ViewModels;

namespace WindowsDev.Commands.NavigationManager.Interfaces
{
    public interface INavigationService
    {
        public void NavigateTo<TViewModel>(params object[] args) where TViewModel : ViewModelBase;
    }
}
