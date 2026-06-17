using WindowsDev.Factories.Interfaces;
using WindowsDev.NavigationManager.Interfaces;
using WindowsDev.ViewModels;

namespace WindowsDev.NavigationManager
{
    public class NavigationService : INavigationService
    {
        private readonly NavigationStore _navigationStore;
        private readonly IViewModelFactory _viewModelFactory;

        public NavigationService(NavigationStore navigationStore, IViewModelFactory viewModelFactory)
        {
            _navigationStore = navigationStore;
            _viewModelFactory = viewModelFactory;
        }

        public async Task NavigateTo<TViewModel>(params object[] args) where TViewModel : ViewModelBase
        {
            _navigationStore.CurrentViewModel = _viewModelFactory.Create<TViewModel>(args);
        }
    }
}
