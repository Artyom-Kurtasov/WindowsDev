using System.Windows.Input;
using WindowsDev.Commands.NavigationManager.Interfaces;
using WindowsDev.Domain.UsersAuthInfo;
using WindowsDev.Infrastructure;

namespace WindowsDev.ViewModels
{
    public class ProjectViewModel : ViewModelBase
    {
        public Project CurrentProject { get; }
        private readonly INavigationService _navigationService;

        public string Name => CurrentProject.Name;
        public string Description => CurrentProject.Description;

        public ICommand SwitchToMainViewCommand { get; }
        public ProjectViewModel(Project project, INavigationService navigationService)
        {
            CurrentProject = project;

            _navigationService = navigationService;

            SwitchToMainViewCommand = new RelayCommand(SwitchToMainView, CanSwitchToMainView);
        }

        private void SwitchToMainView() => _navigationService.NavigateTo<MainWindowViewModel>();
        private bool CanSwitchToMainView() => true;
    }
}
