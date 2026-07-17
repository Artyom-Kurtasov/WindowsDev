using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using Moq;
using WindowsDev.Business.Primitives;
using WindowsDev.Business.Services.Localization.Interfaces;
using WindowsDev.Business.Services.Profile.Interfaces;
using WindowsDev.Business.Services.UserManager;
using WindowsDev.Domain;
using WindowsDev.Domain.DialogsMessages.Errors;
using WindowsDev.Domain.DialogsMessages.Success;
using WindowsDev.Infrastructure;
using WindowsDev.NavigationManager.Interfaces;
using WindowsDev.ViewModels.Auth;
using WindowsDev.ViewModels.Main.Tabs;

namespace WindowsDev.Tests.ViewModels.Main.Tabs
{
    public class ProfileViewModelTest
    {
        private readonly Mock<IProfileService> _profileServiceMock;
        private readonly Mock<IDialogCoordinator> _dialogCoordinatorMock;
        private readonly Mock<INavigationService> _navigationServiceMock;
        private readonly Mock<ILogger<ProfileViewModel>> _loggerMock;
        private readonly Mock<ILanguageChanger> _languageChangerMock;

        private readonly CurrentUserService _currentUser;

        public ProfileViewModelTest()
        {
            _profileServiceMock = new Mock<IProfileService>();
            _dialogCoordinatorMock = new Mock<IDialogCoordinator>();
            _navigationServiceMock = new Mock<INavigationService>();
            _loggerMock = new Mock<ILogger<ProfileViewModel>>();
            _languageChangerMock = new Mock<ILanguageChanger>();

            _currentUser = new CurrentUserService
            {
                UserId = 1,
                Login = "admin",
                Username = "Artyom",
            };

            _languageChangerMock
                .Setup(x => x.Translate(It.IsAny<string>()))
                .Returns((string key) => key);
        }

        private ProfileViewModel CreateViewModel()
        {
            return new ProfileViewModel(
                _currentUser,
                _profileServiceMock.Object,
                _dialogCoordinatorMock.Object,
                _navigationServiceMock.Object,
                _loggerMock.Object,
                _languageChangerMock.Object
            );
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
        public async Task SaveNewUsernameCommand_WhenSuccessful_ShowsSuccessDialog()
        {
            var vm = CreateViewModel();

            vm.Username = "NewUsername";

            _profileServiceMock
                .Setup(x => x.ChangeUsernameAsync("Artyom", "NewUsername"))
                .ReturnsAsync(Result<bool>.Success(true));

            await ((AsyncRelayCommand)vm.SaveNewUsernameCommand).ExecuteAsync(null);

            _dialogCoordinatorMock.Verify(
                x =>
                    x.ShowMessageAsync(
                        vm,
                        DialogTitles.Success,
                        ProfileSuccesses.UsernameChanged,
                        MessageDialogStyle.Affirmative
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task SaveNewPasswordCommand_WhenSuccessful_ShowsSuccessDialog()
        {
            var vm = CreateViewModel();

            vm.CurrentPassword = "OldPassword";
            vm.NewPassword = "NewPassword1!";
            vm.ConfirmPassword = "NewPassword1!";

            _profileServiceMock
                .Setup(x => x.ChangePasswordAsync("OldPassword", "NewPassword1!", "NewPassword1!"))
                .ReturnsAsync(Result<int>.Success(123456));

            await ((AsyncRelayCommand)vm.SaveNewPasswordCommand).ExecuteAsync(null);

            _dialogCoordinatorMock.Verify(
                x =>
                    x.ShowMessageAsync(
                        vm,
                        DialogTitles.Success,
                        $"{ProfileSuccesses.PasswordChanged} 123456",
                        MessageDialogStyle.Affirmative
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task SaveNewUsernameCommand_WhenException_ShowsErrorDialog()
        {
            var vm = CreateViewModel();

            vm.Username = "NewUsername";

            _profileServiceMock
                .Setup(x => x.ChangeUsernameAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception());

            await ((AsyncRelayCommand)vm.SaveNewUsernameCommand).ExecuteAsync(null);

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

        [Fact]
        public async Task SaveNewPasswordCommand_WhenException_ShowsErrorDialog()
        {
            var vm = CreateViewModel();

            vm.CurrentPassword = "OldPassword";
            vm.NewPassword = "NewPassword1!";
            vm.ConfirmPassword = "NewPassword1!";

            _profileServiceMock
                .Setup(x =>
                    x.ChangePasswordAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>()
                    )
                )
                .ThrowsAsync(new Exception());

            await ((AsyncRelayCommand)vm.SaveNewPasswordCommand).ExecuteAsync(null);

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

        [Fact]
        public async Task LogoutCommand_WhenExecuted_NavigatesToAuthorization()
        {
            var vm = CreateViewModel();

            await ((AsyncRelayCommand)vm.LogoutCommand).ExecuteAsync(null);

            _navigationServiceMock.Verify(x => x.NavigateTo<AuthorizationViewModel>(), Times.Once);
        }

        [Fact]
        public async Task SaveNewUsernameCommand_WhenUsernameChangeFails_ShowsWarningDialog()
        {
            var vm = CreateViewModel();

            vm.Username = "NewUsername";

            _profileServiceMock
                .Setup(x => x.ChangeUsernameAsync("Artyom", "NewUsername"))
                .ReturnsAsync(Result<bool>.Failure(It.IsAny<string>()));

            await ((AsyncRelayCommand)vm.SaveNewUsernameCommand).ExecuteAsync(null);

            // NOTE: I use "It.IsAny<string> because all possible errors are checked
            // in ProfileServiceTest.
            _dialogCoordinatorMock.Verify(
                x =>
                    x.ShowMessageAsync(
                        vm,
                        DialogTitles.Warning,
                        It.IsAny<string>(),
                        MessageDialogStyle.Affirmative
                    ),
                Times.Once
            );

            _dialogCoordinatorMock.Verify(
                x =>
                    x.ShowMessageAsync(
                        vm,
                        DialogTitles.Success,
                        ProfileSuccesses.UsernameChanged,
                        MessageDialogStyle.Affirmative
                    ),
                Times.Never
            );
        }

        [Fact]
        public async Task SaveNewPasswordCommand_WhenPasswordChangeFails_ShowsWarningDialog()
        {
            var vm = CreateViewModel();

            vm.CurrentPassword = "OldPassword";
            vm.NewPassword = "NewPassword1!";
            vm.ConfirmPassword = "NewPassword1!";

            _profileServiceMock
                .Setup(x => x.ChangePasswordAsync("OldPassword", "NewPassword1!", "NewPassword1!"))
                .ReturnsAsync(Result<int>.Failure(It.IsAny<string>()));

            await ((AsyncRelayCommand)vm.SaveNewPasswordCommand).ExecuteAsync(null);

            // NOTE: I use "It.IsAny<string> because all possible errors are checked
            // in ProfileServiceTest.
            _dialogCoordinatorMock.Verify(
                x =>
                    x.ShowMessageAsync(
                        vm,
                        DialogTitles.Warning,
                        It.IsAny<string>(),
                        MessageDialogStyle.Affirmative
                    ),
                Times.Once
            );

            _dialogCoordinatorMock.Verify(
                x =>
                    x.ShowMessageAsync(
                        vm,
                        DialogTitles.Success,
                        ProfileSuccesses.PasswordChanged,
                        MessageDialogStyle.Affirmative
                    ),
                Times.Never
            );
        }
    }
}
