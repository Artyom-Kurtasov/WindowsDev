using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using Moq;
using WindowsDev.Business.Services.Localization.Interfaces;
using WindowsDev.Business.Services.ProjectService.Interfaces;
using WindowsDev.Business.Services.UserManager.Interfaces;
using WindowsDev.Domain;
using WindowsDev.Domain.DialogsMessages.Errors;
using WindowsDev.Domain.DialogsMessages.Warnings;
using WindowsDev.Domain.ProjectsModels;
using WindowsDev.Infrastructure;
using WindowsDev.ViewModels.Projects.Dialogs;

namespace WindowsDev.Tests.ViewModels.Projects.Dialogs
{
    public class CreateProjectDialogViewModelTest
    {
        private readonly Mock<IProjectService> _projectServiceMock;
        private readonly Mock<IDialogCoordinator> _dialogCoordinatorMock;
        private readonly Mock<ILogger<CreateProjectDialogViewModel>> _loggerMock;
        private readonly Mock<ILanguageChanger> _languageChangerMock;
        private readonly Mock<ICurrentUserService> _currentUserMock;

        private bool _completedEventWasRaised;
        private bool _closeEventWasRaised;

        public CreateProjectDialogViewModelTest()
        {
            _projectServiceMock = new();
            _dialogCoordinatorMock = new();
            _loggerMock = new();
            _languageChangerMock = new();
            _currentUserMock = new();

            _currentUserMock.SetupGet(x => x.UserId).Returns(1);

            _languageChangerMock
                .Setup(x => x.Translate(It.IsAny<string>()))
                .Returns((string key) => key);
        }

        private CreateProjectDialogViewModel CreateViewModel()
        {
            return new CreateProjectDialogViewModel(
                _dialogCoordinatorMock.Object,
                _currentUserMock.Object,
                _projectServiceMock.Object,
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

            ProjectsInfo? createdProject = null;

            _projectServiceMock
                .Setup(x => x.AddAsync(It.IsAny<ProjectsInfo>()))
                .Callback<ProjectsInfo>(x => createdProject = x)
                .Returns(Task.CompletedTask);

            await ((AsyncRelayCommand)vm.CreateProjectCommand).ExecuteAsync(null);

            Assert.True(_completedEventWasRaised);

            Assert.True(_closeEventWasRaised);

            Assert.NotNull(createdProject);

            Assert.Equal("Test", createdProject.Name);

            Assert.Equal("Description", createdProject.Description);

            Assert.Equal(1, createdProject.UserId);

            _projectServiceMock.Verify(x => x.AddAsync(It.IsAny<ProjectsInfo>()), Times.Once);
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

            _projectServiceMock.Verify(x => x.AddAsync(It.IsAny<ProjectsInfo>()), Times.Never);

            _dialogCoordinatorMock.Verify(
                x =>
                    x.ShowMessageAsync(
                        vm,
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

            _projectServiceMock
                .Setup(x => x.AddAsync(It.IsAny<ProjectsInfo>()))
                .ThrowsAsync(new Exception());

            await ((AsyncRelayCommand)vm.CreateProjectCommand).ExecuteAsync(null);

            Assert.False(_completedEventWasRaised);
            Assert.False(_closeEventWasRaised);

            _projectServiceMock.Verify(x => x.AddAsync(It.IsAny<ProjectsInfo>()), Times.Once);

            _dialogCoordinatorMock.Verify(
                x =>
                    x.ShowMessageAsync(
                        vm,
                        DialogTitles.Error,
                        CommonErrors.UnexpectedError,
                        MessageDialogStyle.Affirmative
                    ),
                Times.Once
            );
        }
    }
}
