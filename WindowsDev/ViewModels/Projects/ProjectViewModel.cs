using System.Collections.ObjectModel;
using System.Windows.Input;
using WindowsDev.Business.Services.TaskService.Interfaces;
using WindowsDev.Commands.NavigationManager.Interfaces;
using WindowsDev.Dialogs;
using WindowsDev.Domain.ProjectsModels;
using WindowsDev.Domain.TasksModels;
using WindowsDev.Infrastructure;
using WindowsDev.ViewModels.Interfaces;
using WindowsDev.ViewModels.Main;
using WindowsDev.ViewModels.Tasks;
using WindowsDev.ViewModels.Tasks.Dialog;
using WindowsDev.Views.Tasks;

namespace WindowsDev.ViewModels.Projects
{
    /// <summary>
    /// ViewModel for a specific project, handles tasks, dialogs, and navigation.
    /// </summary>
    public class ProjectViewModel : ViewModelBase, IInitializableAsync
    {
        private readonly DialogShowingService _dialogShowingService;
        private readonly INavigationService _navigationService;
        private readonly ITaskLoader _taskLoader;

        /// <summary>
        /// The currently selected project.
        /// </summary>
        public ProjectsInfo CurrentProject { get; private set; } = null!;

        /// <summary>
        /// Observable collection of tasks for the current project.
        /// </summary>
        public ObservableCollection<TasksInfo>? TaskItem { get; } = new();

        /// <summary>
        /// Name of the current project.
        /// </summary>
        public string Name => CurrentProject.Name;

        /// <summary>
        /// Description of the current project.
        /// </summary>
        public string? Description => CurrentProject.Description;

        /// <summary>
        /// Command to switch back to the main view.
        /// </summary>
        public ICommand SwitchToMainViewCommand { get; }

        /// <summary>
        /// Command to open the dialog for creating a new task.
        /// </summary>
        public ICommand OpenDialogCommand { get; }

        /// <summary>
        /// Command to open a selected task and load its attachments/comments.
        /// </summary>
        public ICommand OpenTaskCommand { get; }

        public DialogShowingService DialogShowingService => _dialogShowingService;

        /// <summary>
        /// Constructor for ProjectViewModel.
        /// </summary>
        public ProjectViewModel(
            INavigationService navigationService,
            DialogShowingService dialogShowingService,
            ITaskLoader taskLoader)
        {
            _navigationService = navigationService;
            _dialogShowingService = dialogShowingService;
            _taskLoader = taskLoader;

            SwitchToMainViewCommand = new RelayCommand(SwitchToMainView, CanSwitchToMainView);
            OpenDialogCommand = new AsyncRelayCommand(OpenDialog);
            OpenTaskCommand = new AsyncRelayCommandT<TasksInfo>(OpenTask);
        }

        /// <summary>
        /// Opens the selected task, loads attachments and comments, and navigates to TaskViewModel.
        /// </summary>
        private async Task OpenTask(TasksInfo task)
        {
            await _navigationService.NavigateTo<TaskViewModel>(CurrentProject, task);
        }

        /// <summary>
        /// Navigate back to the main window view.
        /// </summary>
        private void SwitchToMainView() => _navigationService.NavigateTo<MainWindowViewModel>();

        /// <summary>
        /// Determines if switching to main view is allowed.
        /// </summary>
        private bool CanSwitchToMainView() => true;

        /// <summary>
        /// Shows the dialog for creating a new task.
        /// </summary>
        private async Task OpenDialog()
        {
            await DialogShowingService.ShowTaskDialogAsync<TaskDialogView, CreateTaskViewModel>(this, CurrentProject.Id);
        }

        public async Task InitializationAsync(params object[] parameters)
        {
            CurrentProject = parameters.OfType<ProjectsInfo>().FirstOrDefault()
                ?? throw new ArgumentNullException(nameof(parameters));

            var tasks = await _taskLoader.LoadTaskAsync(CurrentProject.Id);

            foreach (var task in tasks)
            {
                TaskItem?.Add(task);
            }
        }
    }
}

