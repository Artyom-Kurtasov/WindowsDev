using Microsoft.Extensions.DependencyInjection;
using WindowsDev.Commands.NavigationManager.Interfaces;
using WindowsDev.ViewModels;

namespace WindowsDev.Commands.NavigationManager
{
    public class NavigationService : INavigationService
    {
        private readonly NavigationStore _navigationStore;
        private readonly IServiceProvider _serviceProvider;

        public NavigationService(NavigationStore navigationStore, IServiceProvider serviceProvider)
        {
            _navigationStore = navigationStore;
            _serviceProvider = serviceProvider;
        }

        public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
        {
            var viewModel = _serviceProvider.GetRequiredService<TViewModel>();

            _navigationStore.CurrentViewModel = viewModel;
        }
    }
}
