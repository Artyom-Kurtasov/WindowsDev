using WindowsDev.ViewModels;

namespace WindowsDev.NavigationManager.Interfaces
{
    public interface INavigationService
    {
        public Task NavigateTo<TViewModel>(params object[] args) where TViewModel : ViewModelBase;
    }
}
