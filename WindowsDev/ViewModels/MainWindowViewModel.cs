using System.Collections.ObjectModel;
using System.Windows.Input;
using WindowsDev.Businnes.Services;
using WindowsDev.Businnes.Services.TaskService;
using WindowsDev.Businnes.Services.TaskService.Interfaces;
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
        private readonly ITaskLoader _taskLoader;

        public ObservableCollection<ProjectsInfo>? ProjectList => _sharedDataService.ProjectList;
        public ViewModelBase? CurrentViewModel => _navigationStore.CurrentViewModel;

        public ICommand OpenDialogCommand { get; }
        public ICommand OpenProjectCommand { get; }

        public MainWindowViewModel(
            NavigationStore navigationStore,
            DialogShowingService projectDialogCreator,
            INavigationService navigationService,
            SharedDataService sharedDataService,
            ITaskLoader taskLoader)
        {
            _navigationStore = navigationStore;
            _creator = projectDialogCreator;
            _navigationService = navigationService;
            _sharedDataService = sharedDataService;
            _taskLoader = taskLoader;

            _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;

            SubscribeToServiceProperty(_sharedDataService,
                nameof(_sharedDataService.ProjectList),
                nameof(ProjectList));

            OpenDialogCommand = new AsyncRelayCommand(ShowCreateProjectDialog);
            OpenProjectCommand = new RelayCommandWithParam<ProjectsInfo>(OpenProject, _ => true);
        }
        public async void OpenProject(ProjectsInfo project)
        {
            _sharedDataService.TaskList = await _taskLoader.LoadTaskAsync();
            _navigationService.NavigateTo<ProjectViewModel>(project);
        }

        private async Task ShowCreateProjectDialog()
        {
            await _creator.ShowCreateDialogAsync<CreateProjectDialogView,
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