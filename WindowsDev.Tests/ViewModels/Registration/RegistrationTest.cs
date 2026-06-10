using Moq;
using WindowsDev.Business.DataBase.Interfaces;
using WindowsDev.Business.Services.Registration.Interfaces;
using WindowsDev.Business.Services.Registration.Validation;
using WindowsDev.Commands.NavigationManager.Interfaces;
using WindowsDev.Infrastructure;
using WindowsDev.ViewModels.Auth;
using WindowsDev.ViewModels.Main;

namespace WindowsDev.Tests.ViewModels.Registration
{
    public class RegistrationTest
    {
        private readonly Mock<INavigationService> _navigationServiceMock;
        private readonly Mock<IRegistration> _registrationMock;
        private readonly Mock<IDbManager> _dbManagerMock;
        private readonly UserFieldValidator _userFieldValidator;

        public RegistrationTest()
        {
            _navigationServiceMock = new Mock<INavigationService>();
            _registrationMock = new Mock<IRegistration>();
            _dbManagerMock = new Mock<IDbManager>();

            _userFieldValidator = new UserFieldValidator(_dbManagerMock.Object);
        }

        private RegistrationViewModel CreateViewModel()
        {
            return new RegistrationViewModel(
                _navigationServiceMock.Object,
                _registrationMock.Object,
                _userFieldValidator);
        }

        [Fact]
        public async Task SignUp_WhenAllFieldsValid_NavigatesToMain()
        {
            _registrationMock
                .Setup(x => x.Register("Password123!", "login", "username"))
                .ReturnsAsync(true);

            var vm = CreateViewModel();
            vm.Login = "login";
            vm.Username = "username";
            vm.Password = "Password123!";
            vm.ConfirmPassword = "Password123!";
            vm.IsLoginAvailable = true;
            vm.IsUsernameAvailable = true;

            await ((AsyncRelayCommand)vm.SignUpCommand).ExecuteAsync(null);

            Assert.False(vm.IsRegistrationFailed);
            _navigationServiceMock.Verify(x => x.NavigateTo<MainWindowViewModel>(), Times.Once());
        }

        [Fact]
        public async Task SignUp_WhenServerReturnsError_SetsRegistrationFailed()
        {
            _registrationMock
                .Setup(x => x.Register("Password123!", "login", "username"))
                .ReturnsAsync(false);

            var vm = CreateViewModel();
            vm.Login = "login";
            vm.Username = "username";
            vm.Password = "Password123!";
            vm.ConfirmPassword = "Password123!";
            vm.IsLoginAvailable = true;
            vm.IsUsernameAvailable = true;

            await ((AsyncRelayCommand)vm.SignUpCommand).ExecuteAsync(null);

            Assert.True(vm.IsRegistrationFailed);
            _navigationServiceMock.Verify(x => x.NavigateTo<MainWindowViewModel>(), Times.Never());
        }

        [Fact]
        public async Task SignUp_WhenPasswordsDontMatch_SetsRegistrationFailed()
        {
            var vm = CreateViewModel();
            vm.Login = "login";
            vm.Username = "username";
            vm.Password = "Password123!";
            vm.ConfirmPassword = "DifferentPass123!";
            vm.IsLoginAvailable = true;
            vm.IsUsernameAvailable = true;

            await ((AsyncRelayCommand)vm.SignUpCommand).ExecuteAsync(null);

            Assert.True(vm.IsRegistrationFailed);
            _registrationMock.Verify(
                x => x.Register(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never());
        }

        [Fact]
        public async Task SignUp_WhenLoginNotAvailable_SetsRegistrationFailed()
        {
            var vm = CreateViewModel();
            vm.Login = "login";
            vm.Username = "username";
            vm.Password = "Password123!";
            vm.ConfirmPassword = "Password123!";
            vm.IsLoginAvailable = false;
            vm.IsUsernameAvailable = true;

            await ((AsyncRelayCommand)vm.SignUpCommand).ExecuteAsync(null);

            Assert.True(vm.IsRegistrationFailed);
            _registrationMock.Verify(
                x => x.Register(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never());
        }

        [Fact]
        public async Task SignUp_WhenUsernameNotAvailable_SetsRegistrationFailed()
        {
            var vm = CreateViewModel();
            vm.Login = "login";
            vm.Username = "username";
            vm.Password = "Password123!";
            vm.ConfirmPassword = "Password123!";
            vm.IsLoginAvailable = true;
            vm.IsUsernameAvailable = false;

            await ((AsyncRelayCommand)vm.SignUpCommand).ExecuteAsync(null);

            Assert.True(vm.IsRegistrationFailed);
            _registrationMock.Verify(
                x => x.Register(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never());
        }

        [Theory]
        [InlineData("", "username", "Password123!")]
        [InlineData("login", "", "Password123!")]
        [InlineData("login", "username", "")]
        public async Task SignUp_WhenRequiredFieldsEmpty_SetsRegistrationFailed(
            string login, string username, string password)
        {
            var vm = CreateViewModel();
            vm.Login = login;
            vm.Username = username;
            vm.Password = password;
            vm.ConfirmPassword = password;
            vm.IsLoginAvailable = true;
            vm.IsUsernameAvailable = true;

            await ((AsyncRelayCommand)vm.SignUpCommand).ExecuteAsync(null);

            Assert.True(vm.IsRegistrationFailed);
            _registrationMock.Verify(
                x => x.Register(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never());
        }

        [Theory]
        [InlineData("short")]
        [InlineData("nouppercase1!")]
        [InlineData("NONUMBER!")]
        [InlineData("NoSymbol1")]
        public async Task SignUp_WhenPasswordDoesNotMeetRequirements_SetsRegistrationFailed(string password)
        {
            var vm = CreateViewModel();
            vm.Login = "login";
            vm.Username = "username";
            vm.Password = password;
            vm.ConfirmPassword = password;
            vm.IsLoginAvailable = true;
            vm.IsUsernameAvailable = true;

            await ((AsyncRelayCommand)vm.SignUpCommand).ExecuteAsync(null);

            Assert.True(vm.IsRegistrationFailed);
            _registrationMock.Verify(
                x => x.Register(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never());
        }

        [Fact]
        public void SwitchToAuthView_WhenExecuted_NavigatesToAuthorization()
        {
            var vm = CreateViewModel();

            vm.SwitchToAuthViewCommand.Execute(null);

            _navigationServiceMock.Verify(
                x => x.NavigateTo<AuthorizationViewModel>(),
                Times.Once());
        }

        [Fact]
        public void PropertyChanged_WhenFieldUpdated_ResetsRegistrationFailed()
        {
            var vm = CreateViewModel();
            vm.IsRegistrationFailed = true;

            vm.Login = "newlogin";

            Assert.False(vm.IsRegistrationFailed);
        }

        [Fact]
        public void IsRegistrationFailed_SetAndGet_ReturnsCorrectValue()
        {
            var vm = CreateViewModel();

            vm.IsRegistrationFailed = true;
            Assert.True(vm.IsRegistrationFailed);

            vm.IsRegistrationFailed = false;
            Assert.False(vm.IsRegistrationFailed);
        }
    }
}