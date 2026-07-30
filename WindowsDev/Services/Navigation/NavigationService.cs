using WindowsDev.Factories;
using WindowsDev.ViewModels;
using WindowsDev.ViewModels.Main;

namespace WindowsDev.Services.Navigation
{
    public class NavigationService : INavigationService
    {
        private readonly NavigationStore _navigationStore;
        private readonly IViewModelFactory _viewModelFactory;

        public NavigationService(
            NavigationStore navigationStore,
            IViewModelFactory viewModelFactory
        )
        {
            _navigationStore = navigationStore;
            _viewModelFactory = viewModelFactory;
        }

        public async Task NavigateTo<TViewModel>(params object[] args)
            where TViewModel : ViewModelBase
        {
            if (_navigationStore.CurrentViewModel is MainWindowViewModel viewModel)
            {
                viewModel.Dispose();
            }

            _navigationStore.CurrentViewModel = _viewModelFactory.Create<TViewModel>(args);
        }
    }
}
