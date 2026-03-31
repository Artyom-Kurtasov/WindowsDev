using System.Windows.Input;
using WindowsDev.Businnes.Services;
using WindowsDev.Businnes.Services.ProjectService.Interfaces;
using WindowsDev.Infrastructure;

namespace WindowsDev.ViewModels
{
    public class DialogsViewModel : ViewModelBase, IProjectDialogCreator
    {
        private readonly SharedDataService _sharedDataService;
        private readonly IProjectLoader _projectLoader;
        private readonly IProjectCreator _projectCreator;

        public event Func<Task> Close;

        private string _projectName;
        public string ProjectName
        {
            get => _projectName;
            set
            {
                _projectName = value;
                OnPropertyChanged();
            }
        }
        private string _projectDescription;
        public string ProjectDescription
        {
            get => _projectDescription;
            set
            {
                _projectDescription = value;
                OnPropertyChanged();
            }
        }
        public ICommand CreateProject { get; }
        public DialogsViewModel(IProjectCreator projectCreator, IProjectLoader projectLoader, SharedDataService sharedDataService)
        {
            _projectCreator = projectCreator;
            _projectLoader = projectLoader;
            _sharedDataService = sharedDataService;

            CreateProject = new AsyncRelayCommand(CreateProjectExecute);
        }

        public async Task CreateProjectExecute()
        {
            await _projectCreator.CreateProject(_projectName, _projectDescription, 1);

            if (Close != null)
            {
                await Close.Invoke();
            }

            _sharedDataService.ProjectList = await _projectLoader.LoadProjectAsync();
        }
    }
}
