using System.Collections.ObjectModel;
using System.Windows.Input;
using WindowsDev.Business.Services;
using WindowsDev.Business.Services.TaskService;
using WindowsDev.Business.Services.TaskService.Attachment;
using WindowsDev.Business.Services.TaskService.Interfaces;
using WindowsDev.Commands.NavigationManager.Interfaces;
using WindowsDev.Domain.UsersAuthInfo;
using WindowsDev.Infrastructure;
using WindowsDev.ViewModels.Interfaces;
using WindowsDev.ViewModels.Main;
using WindowsDev.ViewModels.Tasks;
using WindowsDev.Views.Tasks;

namespace WindowsDev.ViewModels.Projects
{
    /// <summary>
    /// ViewModel for a specific project, handles tasks, dialogs, and navigation.
    /// </summary>
    public class ProjectViewModel : ViewModelBase, IInitializableAsync
    {
        private readonly SharedDataService _sharedDataService;
        private readonly FileReader _fileReader;
        private readonly AddComment _addComment;
        private readonly DialogShowingService _dialogShowingService;
        private readonly INavigationService _navigationService;
        private readonly ITaskLoader _taskLoader;

        /// <summary>
        /// The currently selected project.
        /// </summary>
        public ProjectsInfo CurrentProject { get; }

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
            ProjectsInfo project,
            INavigationService navigationService,
            DialogShowingService dialogShowingService,
            ITaskLoader taskLoader,
            SharedDataService sharedDataService,
            FileReader fileReader,
            AddComment addComment)
        {
            CurrentProject = project;

            _navigationService = navigationService;
            _dialogShowingService = dialogShowingService;
            _taskLoader = taskLoader;
            _sharedDataService = sharedDataService;
            _fileReader = fileReader;
            _addComment = addComment;

            SwitchToMainViewCommand = new RelayCommand(SwitchToMainView, CanSwitchToMainView);
            OpenDialogCommand = new AsyncRelayCommand(OpenDialog);
            OpenTaskCommand = new AsyncRelayCommand<TasksInfo>(OpenTask);
        }

        /// <summary>
        /// Opens the selected task, loads attachments and comments, and navigates to TaskViewModel.
        /// </summary>
        private async Task OpenTask(TasksInfo task)
        {
            task.Attachments = await _fileReader.GetAttachmentsAsync(task);
            task.Comments = await _addComment.GetComments(task);
            _sharedDataService.CurrentTask = task;
            await _navigationService.NavigateTo<TaskViewModel>(CurrentProject);
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
            await DialogShowingService.ShowCreateDialogAsync<TaskDialogView, TaskDialogViewModel>(this, CurrentProject.Id);
        }

        public async Task InitializationAsync(params object[] parameters)
        {
            var tasks = await _taskLoader.LoadTaskAsync(CurrentProject.Id);

            foreach (var task in tasks)
            {
                TaskItem?.Add(task);
            }
        }
    }
}

