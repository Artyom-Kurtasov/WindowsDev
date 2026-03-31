using System.Collections.ObjectModel;
using System.Windows.Input;
using WindowsDev.Businnes.Services;
using WindowsDev.Businnes.Services.TaskService;
using WindowsDev.Commands.NavigationManager.Interfaces;
using WindowsDev.Domain.UsersAuthInfo;
using WindowsDev.Infrastructure;
using WindowsDev.View.Controls.WindowsContent;

namespace WindowsDev.ViewModels
{
    public class ProjectViewModel : ViewModelBase
    {
        public ProjectsInfo CurrentProject { get; }

        private readonly DialogShowingService _dialogShowingService;
        private readonly SharedDataService _sharedDataService;
        private readonly AddComment _addComment;
        private readonly INavigationService _navigationService;

        public ObservableCollection<TasksInfo>? TaskItem => _sharedDataService.TaskList;
        public string Name => CurrentProject.Name;
        public string Description => CurrentProject.Description;

        public ICommand SwitchToMainViewCommand { get; }
        public ICommand OpenDialogCommand { get; }
        public ICommand OpenTaskCommand { get; }
        public ProjectViewModel(ProjectsInfo project, SharedDataService sharedDataService, INavigationService navigationService,
            DialogShowingService dialogShowingService, AddComment addComment)
        {
            CurrentProject = project;

            _navigationService = navigationService;
            _sharedDataService = sharedDataService;
            _dialogShowingService = dialogShowingService;
            _addComment = addComment;

            SwitchToMainViewCommand = new RelayCommand(SwitchToMainView, CanSwitchToMainView);
            OpenDialogCommand = new AsyncRelayCommand(OpenDialog);
            OpenTaskCommand = new AsyncRelayCommandWithParam<TasksInfo>(OpenTask);

            SubscribeToServiceProperty(_sharedDataService,
                nameof(_sharedDataService.TaskList),
                nameof(TaskItem));
        }

        private async Task OpenTask(TasksInfo task)
        {
            task.Comments = await _addComment.GetComments(task);
            _navigationService.NavigateTo<TaskViewModel>(task);
        }

        private void SwitchToMainView() => _navigationService.NavigateTo<MainWindowViewModel>();
        private bool CanSwitchToMainView() => true;
        private async Task OpenDialog()
        {
            await _dialogShowingService.ShowCreateDialogAsync<CreateTaskDialogView, TaskDialogViewModel>(this);
        }
    }
}
