using Microsoft.Extensions.Logging;
using Moq;
using WindowsDev.Application.Primitives;
using WindowsDev.Application.Services.Authorization;
using WindowsDev.Application.Services.Localization;
using WindowsDev.Command;
using WindowsDev.Domain.Common;
using WindowsDev.Domain.Common.DialogsMessages.Errors;
using WindowsDev.Services.Dialogs.Interfaces;
using WindowsDev.Services.Navigation;
using WindowsDev.ViewModels.Authorization;
using WindowsDev.ViewModels.Authorization.Dialogs;
using WindowsDev.ViewModels.Main;
using WindowsDev.ViewModels.Registration;
using WindowsDev.Views.Authorization.Dialogs;

namespace WindowsDev.Tests.ViewModels.Authorization
{
    public class AuthorizationViewModelTest
    {
        private readonly Mock<IAuthorization> _authorizationMock;
        private readonly Mock<INavigationService> _navigationServiceMock;
        private readonly Mock<IDialogService> _dialogServiceMock;
        private readonly Mock<ILogger<AuthorizationViewModel>> _loggerMock;
        private readonly Mock<ILanguageChanger> _languageChangerMock;

        public AuthorizationViewModelTest()
        {
            _authorizationMock = new Mock<IAuthorization>();
            _navigationServiceMock = new Mock<INavigationService>();
            _dialogServiceMock = new Mock<IDialogService>();
            _loggerMock = new Mock<ILogger<AuthorizationViewModel>>();
            _languageChangerMock = new Mock<ILanguageChanger>();

            _languageChangerMock
                .Setup(x => x.Translate(It.IsAny<string>()))
                .Returns((string key) => key);
        }

        private AuthorizationViewModel CreateViewModel()
        {
            return new AuthorizationViewModel(
                _navigationServiceMock.Object,
                _authorizationMock.Object,
                _dialogServiceMock.Object,
                _loggerMock.Object,
                _languageChangerMock.Object
            );
        }

        [Fact]
        public async Task AuthorizeCommand_WhenSuccess_NavigateToMain()
        {
            _authorizationMock
                .Setup(x => x.Authorize("login", "password"))
                .ReturnsAsync(Result<bool>.Success(true));

            var vm = CreateViewModel();

            vm.Login = "login";
            vm.Password = "password";

            await ((AsyncRelayCommand)vm.AuthorizeCommand).ExecuteAsync(null);

            Assert.False(vm.HasError);

            _navigationServiceMock.Verify(x => x.NavigateTo<MainWindowViewModel>(), Times.Once);
        }

        [Fact]
        public async Task AuthorizeCommand_WhenFailed_ShowsErrorMessageAndDoNotNavigate()
        {
            _authorizationMock
                .Setup(x => x.Authorize(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(Result<bool>.Failure(AuthErrors.InvalidCredentials));

            var vm = CreateViewModel();

            vm.Login = "login";
            vm.Password = "password";

            await ((AsyncRelayCommand)vm.AuthorizeCommand).ExecuteAsync(null);

            Assert.True(vm.HasError);

            Assert.Equal(AuthErrors.InvalidCredentials, vm.ErrorMessage);

            _navigationServiceMock.Verify(x => x.NavigateTo<MainWindowViewModel>(), Times.Never);
        }

        [Fact]
        public async Task AuthorizeCommand_WhenException_ShowsErrorDialog()
        {
            _authorizationMock
                .Setup(x => x.Authorize(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception());

            var vm = CreateViewModel();

            vm.Login = "login";
            vm.Password = "password";

            await ((AsyncRelayCommand)vm.AuthorizeCommand).ExecuteAsync(null);

            _dialogServiceMock.Verify(
                x => x.ShowErrorDialogAsync(vm, DialogTitles.Error, CommonErrors.UnexpectedError),
                Times.Once
            );
        }

        [Fact]
        public async Task SwitchToRegViewAsync_NavigateToRegistration()
        {
            var vm = CreateViewModel();

            await ((AsyncRelayCommand)vm.SwitchToRegViewCommand).ExecuteAsync(null);

            _navigationServiceMock.Verify(x => x.NavigateTo<RegistrationViewModel>(), Times.Once);
        }

        [Fact]
        public async Task PasswordRecovery_WhenExecuted_ShowsDialog()
        {
            var vm = CreateViewModel();

            await ((AsyncRelayCommand)vm.PasswordRecoveryCommand).ExecuteAsync(null);

            _dialogServiceMock.Verify(
                x => x.ShowDialogAsync<RecoveryCodeDialogView, RecoveryCodeDialogViewModel>(vm),
                Times.Once
            );
        }
    }
}
