using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using Moq;
using WindowsDev.ApiClient.ProfileClient;
using WindowsDev.Application.Common.Localization;
using WindowsDev.Application.Identity.Cryptography;
using WindowsDev.Application.Primitives;
using WindowsDev.Command;
using WindowsDev.Contracts.UI.Profiles;
using WindowsDev.Domain.Messages;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;
using WindowsDev.Domain.Messages.DialogsMessages.Success;
using WindowsDev.Services.Dialogs;
using WindowsDev.Services.Navigation;
using WindowsDev.ViewModels.Authorization;
using WindowsDev.ViewModels.Main.Tabs;

namespace WindowsDev.Tests.ViewModels.Main.Tabs;

public class ProfileViewModelTest
{
    private readonly Mock<ISecureTokenStorage> _tokenStorageMock;
    private readonly Mock<IProfileApiClient> _profileApiClientMock;
    private readonly Mock<IDialogService> _dialogServiceMock;
    private readonly Mock<INavigationService> _navigationServiceMock;
    private readonly Mock<ILogger<ProfileViewModel>> _loggerMock;
    private readonly Mock<ILanguageChanger> _languageChangerMock;

    public ProfileViewModelTest()
    {
        _tokenStorageMock = new Mock<ISecureTokenStorage>();
        _profileApiClientMock = new Mock<IProfileApiClient>();
        _dialogServiceMock = new Mock<IDialogService>();
        _navigationServiceMock = new Mock<INavigationService>();
        _loggerMock = new Mock<ILogger<ProfileViewModel>>();
        _languageChangerMock = new Mock<ILanguageChanger>();

        _languageChangerMock
            .Setup(x => x.Translate(It.IsAny<string>()))
            .Returns((string key) => key);

        _profileApiClientMock
            .Setup(x => x.GetAsync())
            .ReturnsAsync(
                Result<ProfileInfo>.Success(
                    new ProfileInfo (1, "admin", "Artyom")
                )
            );
    }

    private ProfileViewModel CreateViewModel()
    {
        return new ProfileViewModel(
            _tokenStorageMock.Object,
            _profileApiClientMock.Object,
            _dialogServiceMock.Object,
            _navigationServiceMock.Object,
            _loggerMock.Object,
            _languageChangerMock.Object
        );
    }

    [Fact]
    public async Task Constructor_SetsUserData()
    {
        var vm = CreateViewModel();

        await WaitUntilAsync(() => vm.Id == 1);

        Assert.Equal(1, vm.Id);
        Assert.Equal("admin", vm.Login);
        Assert.Equal("Artyom", vm.Username);
    }

    [Fact]
    public async Task SaveNewUsernameCommand_WhenSuccessful_ShowsSuccessDialog()
    {
        var vm = CreateViewModel();
        await WaitUntilAsync(() => vm.Id == 1);

        vm.Username = "NewUsername";

        _profileApiClientMock
            .Setup(x =>
                x.ChangeUsernameAsync(
                    It.Is<ChangeUsernameForm>(f =>
                        f.CurrentUsername == "Artyom" && f.NewUsername == "NewUsername"
                    ),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result<ChangeUsernameInfo>.Success(new ChangeUsernameInfo("token")));

        await ((AsyncRelayCommand)vm.SaveNewUsernameCommand).ExecuteAsync(null);

        _dialogServiceMock.Verify(
            x =>
                x.ShowMessageAsync(
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

        _profileApiClientMock
            .Setup(x =>
                x.ChangePasswordAsync(
                    It.Is<ChangePasswordForm>(f =>
                        f.CurrentPassword == "OldPassword"
                        && f.NewPassword == "NewPassword1!"
                        && f.ConfirmPassword == "NewPassword1!"
                        && f.Login == "admin"
                    ),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                Result<ChangePasswordInfo>.Success(
                    new ChangePasswordInfo(123456, "token")
                )
            );

        await ((AsyncRelayCommand)vm.SaveNewPasswordCommand).ExecuteAsync(null);

        _dialogServiceMock.Verify(
            x =>
                x.ShowMessageAsync(
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
        await WaitUntilAsync(() => vm.Id == 1);

        vm.Username = "NewUsername";

        _profileApiClientMock
            .Setup(x => x.ChangeUsernameAsync(It.IsAny<ChangeUsernameForm>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception());

        await ((AsyncRelayCommand)vm.SaveNewUsernameCommand).ExecuteAsync(null);

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

    [Fact]
    public async Task SaveNewPasswordCommand_WhenException_ShowsErrorDialog()
    {
        var vm = CreateViewModel();

        vm.CurrentPassword = "OldPassword";
        vm.NewPassword = "NewPassword1!";
        vm.ConfirmPassword = "NewPassword1!";

        _profileApiClientMock
            .Setup(x => x.ChangePasswordAsync(It.IsAny<ChangePasswordForm>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception());

        await ((AsyncRelayCommand)vm.SaveNewPasswordCommand).ExecuteAsync(null);

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
        await WaitUntilAsync(() => vm.Id == 1);

        vm.Username = "NewUsername";

        _profileApiClientMock
            .Setup(x => x.ChangeUsernameAsync(It.IsAny<ChangeUsernameForm>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ChangeUsernameInfo>.Failure("some error"));

        await ((AsyncRelayCommand)vm.SaveNewUsernameCommand).ExecuteAsync(null);

        _dialogServiceMock.Verify(
            x =>
                x.ShowMessageAsync(
                    DialogTitles.Warning,
                    It.IsAny<string>(),
                    MessageDialogStyle.Affirmative
                ),
            Times.Once
        );

        _dialogServiceMock.Verify(
            x =>
                x.ShowMessageAsync(
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

        _profileApiClientMock
            .Setup(x => x.ChangePasswordAsync(It.IsAny<ChangePasswordForm>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ChangePasswordInfo>.Failure("some error"));

        await ((AsyncRelayCommand)vm.SaveNewPasswordCommand).ExecuteAsync(null);

        _dialogServiceMock.Verify(
            x =>
                x.ShowMessageAsync(
                    DialogTitles.Warning,
                    It.IsAny<string>(),
                    MessageDialogStyle.Affirmative
                ),
            Times.Once
        );

        _dialogServiceMock.Verify(
            x =>
                x.ShowMessageAsync(
                    DialogTitles.Success,
                    ProfileSuccesses.PasswordChanged,
                    MessageDialogStyle.Affirmative
                ),
            Times.Never
        );
    }

    private static async Task WaitUntilAsync(Func<bool> condition, int timeoutMs = 2000)
    {
        var start = Environment.TickCount;
        while (!condition())
        {
            if (Environment.TickCount - start > timeoutMs)
                throw new TimeoutException("Condition was not met in time.");

            await Task.Delay(10);
        }
    }
}