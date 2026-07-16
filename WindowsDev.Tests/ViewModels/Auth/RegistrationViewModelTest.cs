using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using Moq;
using WindowsDev.Business.Primitives;
using WindowsDev.Business.Services.DebounceService;
using WindowsDev.Business.Services.Localization.Interfaces;
using WindowsDev.Business.Services.Registration.Interfaces;
using WindowsDev.Domain;
using WindowsDev.Domain.DialogsMessages.Errors;
using WindowsDev.Domain.DialogsMessages.Informations;
using WindowsDev.Infrastructure;
using WindowsDev.NavigationManager.Interfaces;
using WindowsDev.ViewModels.Auth;
using WindowsDev.ViewModels.Main;
using Xunit;

namespace WindowsDev.Tests.ViewModels.Auth
{
    public class RegistrationViewModelTest
    {
        private readonly Mock<INavigationService> _navigationServiceMock;
        private readonly Mock<IRegistration> _registrationMock;
        private readonly Mock<IDialogCoordinator> _dialogCoordinatorMock;
        private readonly Mock<ILogger<RegistrationViewModel>> _loggerMock;
        private readonly Mock<IDebounceService> _debounceServiceMock;
        private readonly Mock<ILanguageChanger> _languageChangerMock;


        public RegistrationViewModelTest()
        {
            _navigationServiceMock = new Mock<INavigationService>();
            _registrationMock = new Mock<IRegistration>();
            _dialogCoordinatorMock = new Mock<IDialogCoordinator>();
            _loggerMock = new Mock<ILogger<RegistrationViewModel>>();
            _debounceServiceMock = new Mock<IDebounceService>();
            _languageChangerMock = new Mock<ILanguageChanger>();


            _languageChangerMock
                .Setup(x => x.Translate(It.IsAny<string>()))
                .Returns((string key) => key);


            _debounceServiceMock
                .Setup(x => x.DebounceAsync(
                    It.IsAny<Func<Task>>(),
                    It.IsAny<TimeSpan>()))
                .Returns<Func<Task>, TimeSpan>(async (action, _) =>
                {
                    await action();
                });


            _dialogCoordinatorMock
                .Setup(x => x.ShowMessageAsync(
                    It.IsAny<RegistrationViewModel>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<MessageDialogStyle>(),
                    It.IsAny<MetroDialogSettings>()))
                .ReturnsAsync(MessageDialogResult.Affirmative);
        }


        private RegistrationViewModel CreateViewModel()
        {
            return new RegistrationViewModel(
                _navigationServiceMock.Object,
                _registrationMock.Object,
                _dialogCoordinatorMock.Object,
                _loggerMock.Object,
                _debounceServiceMock.Object,
                _languageChangerMock.Object);
        }


        private void SetupAvailableUsers()
        {
            _registrationMock
                .Setup(x => x.IsLoginAvailableAsync("login"))
                .ReturnsAsync(true);


            _registrationMock
                .Setup(x => x.IsUsernameAvailableAsync("username"))
                .ReturnsAsync(true);
        }


        private void FillValidData(RegistrationViewModel vm)
        {
            vm.Login = "login";
            vm.Username = "username";
            vm.Password = "Password123!";
            vm.ConfirmPassword = "Password123!";
        }


        [Fact]
        public async Task SignUp_WhenSuccess_ShowsInformationDialogAndNavigates()
        {
            SetupAvailableUsers();


            _registrationMock
                .Setup(x => x.Register(
                    "Password123!",
                    "login",
                    "username"))
                .ReturnsAsync(Result<int>.Success(123456));


            var vm = CreateViewModel();

            FillValidData(vm);


            await ((AsyncRelayCommand)vm.SignUpCommand)
                .ExecuteAsync(null);


            _dialogCoordinatorMock.Verify(
                x => x.ShowMessageAsync(
                    vm,
                    DialogTitles.Information,
                    $"{PasswordRecoveryInformations.RecoveryCodeMessage}\n\n123456",
                    MessageDialogStyle.Affirmative,
                    It.IsAny<MetroDialogSettings>()),
                Times.Once);


            _navigationServiceMock.Verify(
                x => x.NavigateTo<MainWindowViewModel>(),
                Times.Once);
        }


        [Fact]
        public async Task SignUp_WhenRegistrationFails_DoesNotNavigate()
        {
            SetupAvailableUsers();


            _registrationMock
                .Setup(x => x.Register(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(Result<int>.Failure("error"));


            var vm = CreateViewModel();

            FillValidData(vm);


            await ((AsyncRelayCommand)vm.SignUpCommand)
                .ExecuteAsync(null);


            _navigationServiceMock.Verify(
                x => x.NavigateTo<MainWindowViewModel>(),
                Times.Never);
        }


        [Fact]
        public async Task SignUp_WhenException_ShowsErrorDialog()
        {
            SetupAvailableUsers();


            _registrationMock
                .Setup(x => x.Register(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ThrowsAsync(new Exception());


            var vm = CreateViewModel();

            FillValidData(vm);


            await ((AsyncRelayCommand)vm.SignUpCommand)
                .ExecuteAsync(null);


            _dialogCoordinatorMock.Verify(
                x => x.ShowMessageAsync(
                    vm,
                    DialogTitles.Error,
                    CommonErrors.UnexpectedError,
                    MessageDialogStyle.Affirmative,
                    It.IsAny<MetroDialogSettings>()),
                Times.Once);
        }


        [Fact]
        public void SignUp_WhenFieldsInvalid_CannotExecute()
        {
            var vm = CreateViewModel();

            Assert.False(
                vm.SignUpCommand.CanExecute(null));
        }


        [Fact]
        public async Task SignUp_WhenPasswordsDoNotMatch_DoesNotRegister()
        {
            SetupAvailableUsers();


            var vm = CreateViewModel();


            vm.Login = "login";
            vm.Username = "username";
            vm.Password = "Password123!";
            vm.ConfirmPassword = "DifferentPassword123!";


            await ((AsyncRelayCommand)vm.SignUpCommand)
                .ExecuteAsync(null);


            _registrationMock.Verify(
                x => x.Register(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);
        }


        [Fact]
        public async Task SignUp_WhenLoginUnavailable_DoesNotRegister()
        {
            _registrationMock
                .Setup(x => x.IsLoginAvailableAsync("login"))
                .ReturnsAsync(false);


            _registrationMock
                .Setup(x => x.IsUsernameAvailableAsync("username"))
                .ReturnsAsync(true);


            var vm = CreateViewModel();

            FillValidData(vm);


            await ((AsyncRelayCommand)vm.SignUpCommand)
                .ExecuteAsync(null);


            _registrationMock.Verify(
                x => x.Register(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);
        }


        [Fact]
        public async Task SignUp_WhenUsernameUnavailable_DoesNotRegister()
        {
            _registrationMock
                .Setup(x => x.IsLoginAvailableAsync("login"))
                .ReturnsAsync(true);


            _registrationMock
                .Setup(x => x.IsUsernameAvailableAsync("username"))
                .ReturnsAsync(false);


            var vm = CreateViewModel();

            FillValidData(vm);


            await ((AsyncRelayCommand)vm.SignUpCommand)
                .ExecuteAsync(null);


            _registrationMock.Verify(
                x => x.Register(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);
        }


        [Fact]
        public async Task SwitchToAuthView_NavigatesToAuthorization()
        {
            var vm = CreateViewModel();


            await ((AsyncRelayCommand)vm.SwitchToAuthViewCommand)
                .ExecuteAsync(null);


            _navigationServiceMock.Verify(
                x => x.NavigateTo<AuthorizationViewModel>(),
                Times.Once);
        }


        [Fact]
        public void LoginChanged_ClearsError()
        {
            var vm = CreateViewModel();


            vm.ErrorMessage = "error";


            vm.Login = "newLogin";


            Assert.False(vm.HasError);
        }
    }
}