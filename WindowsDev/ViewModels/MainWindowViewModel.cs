namespace WindowsDev.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private object? _currentViewModel;
        public object? CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel = value;
                OnPropertyChanged();
            }
        }
    }

}
