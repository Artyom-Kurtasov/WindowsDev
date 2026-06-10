using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using Moq;
using WindowsDev.Business.Services.ProjectService.Interfaces;
using WindowsDev.Business.Services.UserManager;
using WindowsDev.Business.Services.UserManager.Interfaces;
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
        private readonly ICurrentUserService _currentUser;

        private bool _completedEventWasRaised;
        private bool _closeEventWasRaised;

        public CreateProjectDialogViewModelTest()
        {
            _projectServiceMock = new Mock<IProjectService>();
            _dialogCoordinatorMock = new Mock<IDialogCoordinator>();
            _loggerMock = new Mock<ILogger<CreateProjectDialogViewModel>>();

            _currentUser = new CurrentUserService();
        }

        private CreateProjectDialogViewModel CreateViewModel()
        {
            return new CreateProjectDialogViewModel(
                _dialogCoordinatorMock.Object,
                _currentUser,
                _projectServiceMock.Object,
                _loggerMock.Object);
        }

        private void SetupUserAndViewModel(
            CreateProjectDialogViewModel vm,
            string projectName,
            string projectDescription)
        {
            _currentUser.UserId = 1;
            vm.ProjectName = projectName;
            vm.ProjectDescription = projectDescription;
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

        private void SetupDialogCoordinatorMock()
        {
            _dialogCoordinatorMock
                .Setup(x => x.ShowMessageAsync(
                    It.IsAny<CreateProjectDialogViewModel>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(MessageDialogResult.Affirmative);
        }

        [Fact]
        public async Task CreateProject_WhenSuccesful_Addsproject_RaisesEvent_CloseDialog()
        {
            var vm = CreateViewModel();
            SetupUserAndViewModel(vm, "Test", "Description");
            SetupEvents(vm);

            _projectServiceMock
                .Setup(x => x.AddAsync(It.IsAny<ProjectsInfo>()))
                .Returns(Task.CompletedTask);

            await ((AsyncRelayCommand)vm.CreateProjectCommand).ExecuteAsync(null);

            Assert.True(_completedEventWasRaised);
            Assert.True(_closeEventWasRaised);

            _projectServiceMock.Verify(x => x.AddAsync(It.Is<ProjectsInfo>(p =>
                p.Name == "Test" &&
                p.UserId == 1 &&
                p.Description == "Description" &&
                p.CreatedAt.Date == DateTime.Today.Date)), Times.Once);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData(" ")]
        public async Task CreateProject_WhenProjectNameIncorrect_ShowsMessage(string? projectName)
        {
            var vm = CreateViewModel();
            SetupUserAndViewModel(vm, projectName!, "Description");
            SetupEvents(vm);
            SetupDialogCoordinatorMock();

            await ((AsyncRelayCommand)vm.CreateProjectCommand).ExecuteAsync(null);

            Assert.False(_completedEventWasRaised);
            Assert.False(_closeEventWasRaised);

            _projectServiceMock
                .Verify(x => x.AddAsync(It.IsAny<ProjectsInfo>()), Times.Never);

            _dialogCoordinatorMock
                .Verify(x => x.ShowMessageAsync(
                    It.IsAny<CreateProjectDialogViewModel>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task CreateProject_WhenException_LogsErrorAndShowsErrorMessage()
        {
            var vm = CreateViewModel();
            SetupUserAndViewModel(vm, "Test", "Test");
            SetupEvents(vm);

            var loggerWasCalled = false;

            _projectServiceMock
                .Setup(x => x.AddAsync(It.IsAny<ProjectsInfo>()))
                .ThrowsAsync(new Exception());

            SetupDialogCoordinatorMock();

            _loggerMock
                .Setup(x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Callback(() => loggerWasCalled = true);

            await ((AsyncRelayCommand)vm.CreateProjectCommand).ExecuteAsync(null);

            Assert.False(_completedEventWasRaised);
            Assert.False(_closeEventWasRaised);
            Assert.True(loggerWasCalled);

            _projectServiceMock
                .Verify(x => x.AddAsync(It.IsAny<ProjectsInfo>()), Times.Once);

            _dialogCoordinatorMock
                .Verify(x => x.ShowMessageAsync(
                    It.IsAny<CreateProjectDialogViewModel>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()), Times.Once);
        }
    }
}