using WindowsDev.Commands.NavigationManager.Interfaces;
using WindowsDev.Factories.Interfaces;
using WindowsDev.ViewModels;

namespace WindowsDev.Commands.NavigationManager
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

        public void NavigateTo<TViewModel>(params object[] args) where TViewModel : ViewModelBase
        {
            TViewModel viewModel = _viewModelFactory.Create<TViewModel>(args);

            _navigationStore.CurrentViewModel = viewModel;
        }
    }
}


