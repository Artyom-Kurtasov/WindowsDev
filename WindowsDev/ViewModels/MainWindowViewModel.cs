using System.Collections.ObjectModel;
using System.Windows.Input;
using WindowsDev.Businnes.Services;
using WindowsDev.Commands.NavigationManager;
using WindowsDev.Commands.NavigationManager.Interfaces;
using WindowsDev.Domain.UsersAuthInfo;
using WindowsDev.Infrastructure;
using WindowsDev.View.Controls.Users;

namespace WindowsDev.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IDisposable
    {
        private readonly SharedDataService _sharedDataService;
        private readonly DialogShowingService _creator;
        private readonly NavigationStore _navigationStore;
        private readonly INavigationService _navigationService;

        public ObservableCollection<Project>? ProjectList => _sharedDataService.ProjectList;
        public ViewModelBase? CurrentViewModel => _navigationStore.CurrentViewModel;

        public ICommand OpenDialogCommand { get; }
        public ICommand OpenProjectCommand { get; }

        public MainWindowViewModel(
            NavigationStore navigationStore,
            DialogShowingService projectDialogCreator,
            INavigationService navigationService,
            SharedDataService sharedDataService)
        {
            _navigationStore = navigationStore;
            _creator = projectDialogCreator;
            _navigationService = navigationService;
            _sharedDataService = sharedDataService;

            _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;

            SubscribeToServiceProperty(_sharedDataService,
                nameof(_sharedDataService.ProjectList),
                nameof(ProjectList));

            OpenDialogCommand = new AsyncRelayCommand(ShowCreateProjectDialog);

            OpenProjectCommand = new RelayCommandWithParam<Project>(OpenProject, _ => true);
        }
        public void OpenProject(Project project)
        {
            _navigationService.NavigateTo<ProjectViewModel>(project);
        }

        private async Task ShowCreateProjectDialog()
        {
            await _creator.ShowCreateProjectDialogAsync<CreateProjectDialogView,
                DialogsViewModel>(this);
        }

        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }

        public void Dispose()
        {
            _navigationStore.CurrentViewModelChanged -= OnCurrentViewModelChanged;
        }
    }
}