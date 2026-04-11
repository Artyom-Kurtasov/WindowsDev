using System.Windows.Input;
using WindowsDev.Business.Services;
using WindowsDev.Business.Services.ProjectService.Interfaces;
using WindowsDev.Business.Services.UserManager;
using WindowsDev.Infrastructure;

namespace WindowsDev.ViewModels
{
    /// <summary>
    /// ViewModel for dialogs, handles creating new projects.
    /// Implements IProjectDialogCreator for dialog control.
    /// </summary>
    public class DialogsViewModel : ViewModelBase, IProjectDialogCreator
    {
        private readonly CurrentUserData _currentUserData;
        private readonly SharedDataService _sharedDataService;
        private readonly IProjectLoader _projectLoader;
        private readonly IProjectCreator _projectCreator;

        /// <summary>
        /// Event triggered to close the dialog.
        /// </summary>
        public event Func<Task>? Close;

        private string _projectName = string.Empty;
        /// <summary>
        /// Name of the project entered by the user.
        /// </summary>
        public string ProjectName
        {
            get => _projectName;
            set
            {
                _projectName = value;
                OnPropertyChanged(nameof(ProjectName));
            }
        }

        private string _projectDescription = string.Empty;
        /// <summary>
        /// Description of the project entered by the user.
        /// </summary>
        public string ProjectDescription
        {
            get => _projectDescription;
            set
            {
                _projectDescription = value;
                OnPropertyChanged(nameof(ProjectDescription));
            }
        }

        /// <summary>
        /// Command to create a new project asynchronously.
        /// </summary>
        public ICommand CreateProject { get; }

        /// <summary>
        /// Constructor for DialogsViewModel.
        /// </summary>
        public DialogsViewModel(
            IProjectCreator projectCreator,
            IProjectLoader projectLoader,
            SharedDataService sharedDataService,
            CurrentUserData currentUserData)
        {
            _projectCreator = projectCreator;
            _projectLoader = projectLoader;
            _sharedDataService = sharedDataService;
            _currentUserData = currentUserData;

            CreateProject = new AsyncRelayCommand(CreateProjectExecute);
        }

        /// <summary>
        /// Executes project creation and updates shared project list.
        /// </summary>
        public async Task CreateProjectExecute()
        {
            await _projectCreator.CreateProject(_projectName, _projectDescription, _currentUserData.UserId);

            if (Close != null)
                await Close.Invoke();

            _sharedDataService.ProjectList = await _projectLoader.LoadProjectAsync();
        }
    }
}

