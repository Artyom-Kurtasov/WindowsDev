using WindowsDev.Commands.NavigationManager.Interfaces;
using WindowsDev.Factories.Interfaces;
using WindowsDev.ViewModels;
using WindowsDev.ViewModels.Interfaces;

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

        public async Task NavigateTo<TViewModel>(params object[] args) where TViewModel : ViewModelBase
        {
            TViewModel viewModel = _viewModelFactory.Create<TViewModel>(args);

            if (viewModel is IInitializableAsync initializableAsync)
            {
                await initializableAsync.InitializationAsync();
            }

            _navigationStore.CurrentViewModel = viewModel;
        }
    }
}


