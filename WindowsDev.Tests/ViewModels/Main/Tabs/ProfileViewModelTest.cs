using MahApps.Metro.Controls.Dialogs;
using Moq;
using WindowsDev.Business.Services.Profile.Interfaces;
using WindowsDev.Business.Services.UserManager;
using WindowsDev.Business.Services.UserManager.Interfaces;
using WindowsDev.Commands.NavigationManager.Interfaces;
using WindowsDev.Infrastructure;
using WindowsDev.ViewModels.Main.Tabs;

namespace WindowsDev.Tests.ViewModels.Main.Tabs
{
    public class ProfileViewModelTest
    {
        private readonly Mock<IProfileService> _profileServiceMock;
        private readonly Mock<IDialogCoordinator> _dialogCoordinatorMock;
        private readonly Mock<INavigationService> _navigationServiceMock;
        private readonly ICurrentUserService _currentUser;

        public ProfileViewModelTest()
        {
            _profileServiceMock = new Mock<IProfileService>();
            _dialogCoordinatorMock = new Mock<IDialogCoordinator>();
            _navigationServiceMock = new Mock<INavigationService>();

            _currentUser = new CurrentUserService
            {
                UserId = 1,
                Login = "admin",
                Username = "Artyom"
            };
        }

        private ProfileViewModel CreateViewModel()
        {
            return new ProfileViewModel(
                _currentUser,
                _profileServiceMock.Object,
                _dialogCoordinatorMock.Object,
                _navigationServiceMock.Object);
        }

        private void SetupDialogCoordinatorMock()
        {
            _dialogCoordinatorMock
                .Setup(x => x.ShowMessageAsync(
                    It.IsAny<ProfileViewModel>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<MessageDialogStyle>(),
                    It.IsAny<MetroDialogSettings>()))
                .ReturnsAsync(MessageDialogResult.Affirmative);
        }

        [Fact]
        public void Constructor_SetsUserData()
        {
            var vm = CreateViewModel();

            Assert.Equal(1, vm.Id);
            Assert.Equal("admin", vm.Login);
            Assert.Equal("Artyom", vm.Username);
        }

        [Fact]
        public async Task SaveNewUsername_WhenSuccessful_CallsService()
        {
            var vm = CreateViewModel();

            vm.Username = "NewUsername";

            _profileServiceMock
                .Setup(x => x.ChangeUsernameAsync("Artyom", "NewUsername"))
                .Returns(Task.CompletedTask);

            await ((AsyncRelayCommand)vm.SaveNewUsernameCommand)
                .ExecuteAsync(null);

            _profileServiceMock.Verify(
                x => x.ChangeUsernameAsync("Artyom", "NewUsername"),
                Times.Once);
        }

        [Fact]
        public async Task SaveNewPassword_WhenSuccessful_CallsService()
        {
            var vm = CreateViewModel();

            vm.CurrentPassword = "OldPassword";
            vm.NewPassword = "NewPassword1!";
            vm.ConfirmPassword = "NewPassword1!";

            _profileServiceMock
                .Setup(x => x.ChangePasswordAsync(
                    "OldPassword",
                    "NewPassword1!",
                    "NewPassword1!"))
                .Returns(Task.CompletedTask);

            await ((AsyncRelayCommand)vm.SaveNewPasswordCommand)
                .ExecuteAsync(null);

            _profileServiceMock.Verify(
                x => x.ChangePasswordAsync(
                    "OldPassword",
                    "NewPassword1!",
                    "NewPassword1!"),
                Times.Once);
        }

        [Fact]
        public async Task SaveNewUsername_WhenException_ShowsMessage()
        {
            var vm = CreateViewModel();

            vm.Username = "NewUsername";

            SetupDialogCoordinatorMock();

            _profileServiceMock
                .Setup(x => x.ChangeUsernameAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ThrowsAsync(new Exception("Error_Username"));

            await ((AsyncRelayCommand)vm.SaveNewUsernameCommand)
                .ExecuteAsync(null);

            _profileServiceMock.Verify(
                x => x.ChangeUsernameAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Once);

            _dialogCoordinatorMock.Verify(
                x => x.ShowMessageAsync(
                    It.IsAny<ProfileViewModel>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<MessageDialogStyle>(),
                    It.IsAny<MetroDialogSettings>()),
                Times.Once);
        }

        [Fact]
        public async Task SaveNewPassword_WhenException_ShowsMessage()
        {
            var vm = CreateViewModel();

            vm.CurrentPassword = "OldPassword";
            vm.NewPassword = "NewPassword";
            vm.ConfirmPassword = "NewPassword";

            SetupDialogCoordinatorMock();

            _profileServiceMock
                .Setup(x => x.ChangePasswordAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ThrowsAsync(new Exception("Error_Password"));

            await ((AsyncRelayCommand)vm.SaveNewPasswordCommand)
                .ExecuteAsync(null);

            _profileServiceMock.Verify(
                x => x.ChangePasswordAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Once);

            _dialogCoordinatorMock.Verify(
                x => x.ShowMessageAsync(
                    It.IsAny<ProfileViewModel>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<MessageDialogStyle>(),
                    It.IsAny<MetroDialogSettings>()),
                Times.Once);
        }
    }
}