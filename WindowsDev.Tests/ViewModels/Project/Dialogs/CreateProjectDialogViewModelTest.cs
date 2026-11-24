using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using Moq;
using WindowsDev.ApiClient.ProjectsClient;
using WindowsDev.Application.Common.Localization;
using WindowsDev.Application.Primitives;
using WindowsDev.Command;
using WindowsDev.Contracts.UI.Projects;
using WindowsDev.Domain.Messages;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;
using WindowsDev.Domain.Messages.DialogsMessages.Warnings;
using WindowsDev.Services.Dialogs;
using WindowsDev.ViewModels.Projects.Dialogs;

namespace WindowsDev.Tests.ViewModels.Projects.Dialogs;

public class CreateProjectDialogViewModelTest
{
    private readonly Mock<IProjectsApiClient> _projectApiClientMock;
    private readonly Mock<IDialogService> _dialogServiceMock;
    private readonly Mock<ILogger<CreateProjectDialogViewModel>> _loggerMock;
    private readonly Mock<ILanguageChanger> _languageChangerMock;

    private bool _completedEventWasRaised;
    private bool _closeEventWasRaised;

    public CreateProjectDialogViewModelTest()
    {
        _projectApiClientMock = new();
        _dialogServiceMock = new();
        _loggerMock = new();
        _languageChangerMock = new();

        _languageChangerMock
            .Setup(x => x.Translate(It.IsAny<string>()))
            .Returns((string key) => key);
    }

    private CreateProjectDialogViewModel CreateViewModel()
    {
        return new CreateProjectDialogViewModel(
            _dialogServiceMock.Object,
            _projectApiClientMock.Object,
            _loggerMock.Object,
            _languageChangerMock.Object
        );
    }

    private void SetupEvents(CreateProjectDialogViewModel vm)
    {
        _completedEventWasRaised = false;
        _closeEventWasRaised = false;

        vm.Completed += () =>
        {
            _completedEventWasRaised = true;
            return Task.CompletedTask;
        };

        vm.CloseRequested += () =>
        {
            _closeEventWasRaised = true;
            return Task.CompletedTask;
        };
    }

    [Fact]
    public async Task CreateProjectCommand_WhenValid_CreatesProjectAndRaisesCompleted()
    {
        var vm = CreateViewModel();

        SetupEvents(vm);

        vm.ProjectName = "Test";
        vm.ProjectDescription = "Description";

        ProjectForm? capturedForm = null;

        _projectApiClientMock
            .Setup(x => x.AddAsync(It.IsAny<ProjectForm>(), It.IsAny<CancellationToken>()))
            .Callback<ProjectForm, CancellationToken>((form, _) => capturedForm = form)
            .ReturnsAsync(Result<ProjectInfo>.Success(
                new ProjectInfo(1, "Test", "Description", DateTime.UtcNow)));

        await ((AsyncRelayCommand)vm.CreateProjectCommand).ExecuteAsync(null);

        Assert.True(_completedEventWasRaised);
        Assert.True(_closeEventWasRaised);
        Assert.NotNull(capturedForm);

        Assert.Equal("Test", capturedForm.Name);
        Assert.Equal("Description", capturedForm.Description);

        _projectApiClientMock.Verify(
            x => x.AddAsync(It.IsAny<ProjectForm>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData(" ")]
    public async Task CreateProjectCommand_WhenNameEmpty_ShowsWarning(string? name)
    {
        var vm = CreateViewModel();

        SetupEvents(vm);

        vm.ProjectName = name!;
        vm.ProjectDescription = "Description";

        await ((AsyncRelayCommand)vm.CreateProjectCommand).ExecuteAsync(null);

        Assert.False(_completedEventWasRaised);
        Assert.False(_closeEventWasRaised);

        _projectApiClientMock.Verify(x => x.AddAsync(It.IsAny<ProjectForm>()), Times.Never);

        _dialogServiceMock.Verify(
            x =>
                x.ShowMessageAsync(
                    DialogTitles.Warning,
                    CreateProjectWarnings.EnterName,
                    MessageDialogStyle.Affirmative
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task CreateProjectCommand_WhenServiceThrows_ShowsErrorDialog()
    {
        var vm = CreateViewModel();

        SetupEvents(vm);

        vm.ProjectName = "Test";
        vm.ProjectDescription = "Description";

        _projectApiClientMock
            .Setup(x => x.AddAsync(It.IsAny<ProjectForm>()))
            .ThrowsAsync(new Exception());

        await ((AsyncRelayCommand)vm.CreateProjectCommand).ExecuteAsync(null);

        Assert.False(_completedEventWasRaised);
        Assert.False(_closeEventWasRaised);

        _projectApiClientMock.Verify(x => x.AddAsync(It.IsAny<ProjectForm>()), Times.Once);

        _dialogServiceMock.Verify(
            x =>
                x.ShowMessageAsync(
                    DialogTitles.Error,
                    CommonErrors.UnexpectedError,
                    MessageDialogStyle.Affirmative
                ),
            Times.Once
        );
    }
}