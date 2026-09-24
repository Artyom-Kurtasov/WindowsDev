using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using System.Windows.Input;
using WindowsDev.Api.DTO.Request.ProjectService;
using WindowsDev.ApiClients.ProjectsClient;
using WindowsDev.Application.Common.Utils.Localization;
using WindowsDev.Application.Identity;
using WindowsDev.Command;
using WindowsDev.Domain.Messages;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;
using WindowsDev.Domain.Messages.DialogsMessages.Warnings;
using WindowsDev.Infrastructure.Logging;
using WindowsDev.Services.Dialogs;

namespace WindowsDev.ViewModels.Projects.Dialogs;

internal class CreateProjectDialogViewModel : LocalizedViewModelBase, IDialogViewModel
{
    private readonly IUserSession _userSession;
    private readonly IProjectsApiClient _projectsApiClient;
    private readonly IDialogCoordinator _dialogCoordinator;
    private readonly ILogger<CreateProjectDialogViewModel> _logger;

    public CreateProjectDialogViewModel(
        IDialogCoordinator dialogCoordinator,
        IProjectsApiClient projectsApiClient,
        ILogger<CreateProjectDialogViewModel> logger,
        ILanguageChanger languageChanger,
        IUserSession userSession
    )
        : base(languageChanger)
    {
        _dialogCoordinator = dialogCoordinator;
        _projectsApiClient = projectsApiClient;
        _logger = logger;
        _userSession = userSession;

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

    private async Task CreateProjectAsync()
    {
        if (string.IsNullOrWhiteSpace(ProjectName))
        {
            await _dialogCoordinator.ShowMessageAsync(
                this,
                Translate(DialogTitles.Warning),
                Translate(CreateProjectWarnings.EnterName),
                MessageDialogStyle.Affirmative
            );

            return;
        }

        try
        {
            await _projectsApiClient.AddAsync(
                new AddProjectRequest
                {
                    Name = ProjectName,
                    Description = ProjectDescription
                }
            );

            if (Completed != null)
                await Completed.Invoke();

            await CloseDialogAsync();
        }
        catch (Exception ex)
        {
            ProjectLogs.ProjectCreationFailed(_logger, ProjectName, ex);

            await _dialogCoordinator.ShowMessageAsync(
                this,
                Translate(DialogTitles.Error),
                Translate(CommonErrors.UnexpectedError),
                MessageDialogStyle.Affirmative
            );
        }
    }

    private async Task CloseDialogAsync()
    {
        if (CloseRequested != null)
            await CloseRequested.Invoke();
    }
}