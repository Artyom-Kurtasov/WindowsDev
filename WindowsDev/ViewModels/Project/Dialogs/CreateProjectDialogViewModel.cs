using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using System.Windows.Input;
using WindowsDev.Business.Services.Localization.Interfaces;
using WindowsDev.Business.Services.ProjectService.Interfaces;
using WindowsDev.Business.Services.UserManager.Interfaces;
using WindowsDev.Dialogs.Interfaces;
using WindowsDev.Domain;
using WindowsDev.Domain.DialogsMessages.Errors;
using WindowsDev.Domain.DialogsMessages.Warnings;
using WindowsDev.Domain.ProjectsModels;
using WindowsDev.Infrastructure;
using WindowsDev.Infrastructure.Logging;

namespace WindowsDev.ViewModels.Projects.Dialogs
{
    public class CreateProjectDialogViewModel : LocalizedViewModelBase, IDialogViewModel
    {
        private readonly ICurrentUserService _currentUserData;
        private readonly IProjectService _projectService;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly ILogger<CreateProjectDialogViewModel> _logger;

        public CreateProjectDialogViewModel(
            IDialogCoordinator dialogCoordinator,
            ICurrentUserService currentUserData,
            IProjectService projectService,
            ILogger<CreateProjectDialogViewModel> logger,
            ILanguageChanger languageChanger) : base(languageChanger)
        {
            _dialogCoordinator = dialogCoordinator;
            _currentUserData = currentUserData;
            _projectService = projectService;
            _logger = logger;

            CloseDialogCommand = new AsyncRelayCommand(CloseDialogAsync);
            CreateProjectCommand = new AsyncRelayCommand(CreateProjectAsync);
        }


        public ICommand CloseDialogCommand { get; }
        public ICommand CreateProjectCommand { get; }


        private string _projectName = string.Empty;

        public string ProjectName
        {
            get => _projectName;
            set
            {
                if (_projectName == value)
                    return;

                _projectName = value;
                OnPropertyChanged(nameof(ProjectName));
            }
        }


        private string _projectDescription = string.Empty;

        public string ProjectDescription
        {
            get => _projectDescription;
            set
            {
                if (_projectDescription == value)
                    return;

                _projectDescription = value;
                OnPropertyChanged(nameof(ProjectDescription));
            }
        }


        public event Func<Task>? CloseRequested;
        public event Func<Task>? Completed;


        public async Task CreateProjectAsync()
        {
            if (string.IsNullOrWhiteSpace(ProjectName))
            {
                await _dialogCoordinator.ShowMessageAsync(
                    this,
                    Translate(DialogTitles.Warning),
                    Translate(CreateProjectWarnings.EnterName),
                    MessageDialogStyle.Affirmative);

                return;
            }

            try
            {
                await _projectService.AddAsync(new ProjectsInfo
                {
                    Name = ProjectName,
                    UserId = _currentUserData.UserId,
                    Description = ProjectDescription,
                    CreatedAt = DateTime.Today.ToUniversalTime()
                });

                if (Completed != null)
                    await Completed.Invoke();
            }
            catch (Exception ex)
            {
                ProjectLogs.ProjectCreationFailed(_logger, ProjectName, ex);

                await _dialogCoordinator.ShowMessageAsync(
                    this,
                    Translate(DialogTitles.Error),
                    Translate(CommonErrors.UnexpectedError),
                    MessageDialogStyle.Affirmative);
            }
        }


        private async Task CloseDialogAsync()
        {
            if (CloseRequested != null)
                await CloseRequested.Invoke();
        }
    }
}