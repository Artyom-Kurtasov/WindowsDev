using MahApps.Metro.Controls.Dialogs;
using Moq;
using WindowsDev.Business.Services.Authorization.Interfaces;
using WindowsDev.Dialogs.Interfaces;
using WindowsDev.Infrastructure;
using WindowsDev.NavigationManager.Interfaces;
using WindowsDev.ViewModels.Auth;
using WindowsDev.ViewModels.Auth.Dialogs;
using WindowsDev.ViewModels.Main;
using WindowsDev.Views.Auth.Dialogs;

namespace WindowsDev.Tests.ViewModels.Authorization
{
    public class AuthorizationViewModelTest
    {
        private readonly Mock<IAuthorization> _authorizationMock;
        private readonly Mock<INavigationService> _navigationServiceMock;
        private readonly Mock<IDialogService> _dialogServiceMock;
        private readonly Mock<IDialogCoordinator> _dialogCoordinatorMock;

        public AuthorizationViewModelTest()
        {
            _authorizationMock = new Mock<IAuthorization>();
            _navigationServiceMock = new Mock<INavigationService>();
            _dialogServiceMock = new Mock<IDialogService>();
            _dialogCoordinatorMock = new Mock<IDialogCoordinator>();
        }

        [Fact]
        public async Task Authorize_WhenSuccess_NavigateToMain()
        {
            _authorizationMock
                .Setup(x => x.Authorize("login", "password"))
                .Returns(Task.CompletedTask);

            var vm = new AuthorizationViewModel(
                _navigationServiceMock.Object,
                _authorizationMock.Object,
                _dialogServiceMock.Object,
                _dialogCoordinatorMock.Object)
            {
                Login = "login",
                Password = "password"
            };

            await ((AsyncRelayCommand)vm.AuthorizeCommand).ExecuteAsync(null);

            Assert.False(vm.IsLoginFailed);
            _navigationServiceMock.Verify(x => x.NavigateTo<MainWindowViewModel>(), Times.Once());
        }

        [Fact]
        public async Task Authorize_WhenFailed_DoNotNavigateToMain()
        {
            _authorizationMock
                .Setup(x => x.Authorize(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Authorization failed"));

            _dialogCoordinatorMock
                .Setup(x => x.ShowMessageAsync(
                    It.IsAny<AuthorizationViewModel>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<MessageDialogStyle>(),
                    It.IsAny<MetroDialogSettings>()))
                .ReturnsAsync(MessageDialogResult.Affirmative);

            var vm = new AuthorizationViewModel(
                _navigationServiceMock.Object,
                _authorizationMock.Object,
                _dialogServiceMock.Object,
                _dialogCoordinatorMock.Object)
            {
                Login = "login",
                Password = "password"
            };

            await ((AsyncRelayCommand)vm.AuthorizeCommand).ExecuteAsync(null);

            Assert.True(vm.IsLoginFailed);
            _navigationServiceMock.Verify(x => x.NavigateTo<MainWindowViewModel>(), Times.Never());
        }

        [Fact]
        public async Task SwitchToRegViewAsync_NavigateToRegistration()
        {
            var vm = new AuthorizationViewModel(
                _navigationServiceMock.Object,
                _authorizationMock.Object,
                _dialogServiceMock.Object,
                _dialogCoordinatorMock.Object);

            await ((AsyncRelayCommand)vm.SwitchToRegViewCommand).ExecuteAsync(null);

            _navigationServiceMock.Verify(x => x.NavigateTo<RegistrationViewModel>(), Times.Once());
        }

        [Fact]
        public async Task PasswordRecovery_WhenExecuted_ShowsDialog()
        {
            var vm = new AuthorizationViewModel(
                _navigationServiceMock.Object,
                _authorizationMock.Object,
                _dialogServiceMock.Object,
                _dialogCoordinatorMock.Object);

            await ((AsyncRelayCommand)vm.PasswordRecoveryCommand).ExecuteAsync(null);

            _dialogServiceMock.Verify(
                x => x.ShowDialogAsync<RecoveryCodeDialogView, RecoveryCodeDialogViewModel>(
                    It.IsAny<AuthorizationViewModel>()),
                Times.Once());
        }
    }
}