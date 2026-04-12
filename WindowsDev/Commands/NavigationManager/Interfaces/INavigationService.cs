using WindowsDev.ViewModels;

namespace WindowsDev.Commands.NavigationManager.Interfaces
{
    public interface INavigationService
    {
        public Task NavigateTo<TViewModel>(params object[] args) where TViewModel : ViewModelBase;
    }
}
