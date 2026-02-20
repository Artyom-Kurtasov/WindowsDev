using WindowsDev.Commands.NavigationManager;

namespace WindowsDev.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly NavigationStore _navigationStore;

        public ViewModelBase? CurrentViewModel => _navigationStore.CurrentViewModel;

        public MainWindowViewModel(NavigationStore navigationStore)
        {
            _navigationStore = navigationStore;

            _navigationStore.CurrentViewModelChanged += OnCurrentVIewModelChanged;
        }

        private void OnCurrentVIewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }
    }

}
