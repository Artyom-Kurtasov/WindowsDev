using WindowsDev.ViewModels;

namespace WindowsDev.Services.Navigation
{
    public interface INavigationService
    {
        public Task NavigateTo<TViewModel>(params object[] args) where TViewModel : ViewModelBase;
    }
}
