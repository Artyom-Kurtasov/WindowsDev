using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using System.Windows.Input;
using WindowsDev.Business.Services.ProjectService.Interfaces;
using WindowsDev.Business.Services.UserManager.Interfaces;
using WindowsDev.Dialogs.Interfaces;
using WindowsDev.Domain.ProjectsModels;
using WindowsDev.Infrastructure;

namespace WindowsDev.ViewModels.Projects.Dialogs
{
    public class CreateProjectDialogViewModel : ViewModelBase, IDialogViewModel
    {
        private readonly ICurrentUserService _currentUserData;
        private readonly IProjectService _projectService;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly ILogger<CreateProjectDialogViewModel> _logger;

        public CreateProjectDialogViewModel(IDialogCoordinator dialogCoordinator,
            ICurrentUserService currentUserData,
            IProjectService projectService,
            ILogger<CreateProjectDialogViewModel> logger)
        {
            _projectService = projectService;
            _currentUserData = currentUserData;
            _dialogCoordinator = dialogCoordinator;
            _logger = logger;

            CloseDialogCommand = new AsyncRelayCommand(CloseDialog);
            CreateProjectCommand = new AsyncRelayCommand(CreateProjectExecute);
        }

        public event Func<Task>? CloseRequested;
        public event Func<Task>? Completed;

        public ICommand CloseDialogCommand { get; }
        public ICommand CreateProjectCommand { get; }

        private string _projectName = string.Empty;
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
        public string ProjectDescription
        {
            get => _projectDescription;
            set
            {
                _projectDescription = value;
                OnPropertyChanged(nameof(ProjectDescription));
            }
        }

        public async Task CreateProjectExecute()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(ProjectName))
                {
                    await _projectService.AddAsync(new ProjectsInfo
                    {
                        Name = _projectName,
                        UserId = _currentUserData.UserId,
                        Description = _projectDescription,
                        CreatedAt = DateTime.Today.Date.ToUniversalTime()
                    });

                    // Notify parent ViewModel to refresh before closing
                    Completed?.Invoke();
                    await CloseDialog();
                }
                else
                {
                    await _dialogCoordinator.ShowMessageAsync(this,
                        Translate("Warning_Title"),
                        Translate("Warning_EnterName"),
                        MessageDialogStyle.Affirmative);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create project");
                await _dialogCoordinator.ShowMessageAsync(this,
                    Translate("Error_Title"),
                    Translate(ex.Message),
                    MessageDialogStyle.Affirmative);
            }
        }

        private async Task CloseDialog()
        {
            if (CloseRequested != null)
                await CloseRequested.Invoke();
        }
    }
}