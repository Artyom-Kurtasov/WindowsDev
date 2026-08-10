using WindowsDev.ViewModels;

namespace WindowsDev.Services.Navigation
{
    public interface INavigationService
    {
        Task NavigateTo<TViewModel>(params object[] args) where TViewModel : ViewModelBase;
    }
}
