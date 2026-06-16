using Moq;
using WindowsDev.Business.Services.Authorization.Interfaces;
using WindowsDev.Commands.NavigationManager.Interfaces;
using WindowsDev.Dialogs.Interfaces;
using WindowsDev.Infrastructure;
using WindowsDev.ViewModels.Auth;
using WindowsDev.ViewModels.Main;

namespace WindowsDev.Tests.ViewModels.Authorization
{
    public class AuthorizationViewModelTest
    {
        private readonly Mock<IAuthorization> _authorizationMock;
        private readonly Mock<INavigationService> _navigationServiceMock;
        private readonly Mock<IDialogService> _dialogServiceMock;

        public AuthorizationViewModelTest()
        {
            _authorizationMock = new Mock<IAuthorization>();
            _navigationServiceMock = new Mock<INavigationService>();
            _dialogServiceMock = new Mock<IDialogService>();
        }

        [Fact]
        public async Task Authorize_WhenSuccess_NavigateToMain()
        {
            _authorizationMock
                .Setup(x => x.Authorize("login", "password"))
                .ReturnsAsync(true);

            var vm = new AuthorizationViewModel(
                _navigationServiceMock.Object,
                _authorizationMock.Object,
                _dialogServiceMock.Object)
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
                .ReturnsAsync(false);

            var vm = new AuthorizationViewModel(
                _navigationServiceMock.Object,
                _authorizationMock.Object,
                _dialogServiceMock.Object)
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
                _dialogServiceMock.Object);

            await ((AsyncRelayCommand)vm.SwitchToRegViewCommand).ExecuteAsync(null);

            _navigationServiceMock.Verify(x => x.NavigateTo<RegistrationViewModel>(), Times.Once());
        }
    }
}