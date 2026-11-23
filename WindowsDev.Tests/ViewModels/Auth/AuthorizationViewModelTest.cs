using Microsoft.Extensions.Logging;
using Moq;
using WindowsDev.ApiClient.AuthClient;
using WindowsDev.Application.Common.Localization;
using WindowsDev.Application.Identity.Cryptography;
using WindowsDev.Application.Primitives;
using WindowsDev.Command;
using WindowsDev.Contracts.UI.Identity;
using WindowsDev.Domain.Entities;
using WindowsDev.Domain.Messages;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;
using WindowsDev.Services.Dialogs;
using WindowsDev.Services.Navigation;
using WindowsDev.ViewModels.Authorization;
using WindowsDev.ViewModels.Authorization.Dialogs;
using WindowsDev.ViewModels.Main;
using WindowsDev.ViewModels.Registration;
using WindowsDev.Views.Authorization.Dialogs;

namespace WindowsDev.Tests.ViewModels.Authorization;

public class AuthorizationViewModelTest
{
    private readonly Mock<IAuthApiClient> _authApiClientMock;
    private readonly Mock<INavigationService> _navigationServiceMock;
    private readonly Mock<IDialogService> _dialogServiceMock;
    private readonly Mock<ILogger<AuthorizationViewModel>> _loggerMock;
    private readonly Mock<ILanguageChanger> _languageChangerMock;
    private readonly Mock<ISecureTokenStorage> _secureTokenStorageMock;

    public AuthorizationViewModelTest()
    {
        _authApiClientMock = new Mock<IAuthApiClient>();
        _navigationServiceMock = new Mock<INavigationService>();
        _dialogServiceMock = new Mock<IDialogService>();
        _loggerMock = new Mock<ILogger<AuthorizationViewModel>>();
        _languageChangerMock = new Mock<ILanguageChanger>();
        _secureTokenStorageMock = new Mock<ISecureTokenStorage>();

        _languageChangerMock
            .Setup(x => x.Translate(It.IsAny<string>()))
            .Returns((string key) => key);
    }

    private AuthorizationViewModel CreateViewModel() =>
        new AuthorizationViewModel(
            _navigationServiceMock.Object,
            _dialogServiceMock.Object,
            _authApiClientMock.Object,
            _loggerMock.Object,
            _languageChangerMock.Object,
            _secureTokenStorageMock.Object
        );

    private static UserRefreshToken CreateRefreshToken(string token) =>
        new UserRefreshToken
        {
            Id = 1,
            UserId = 42,
            Token = token,
            IsRevoked = false,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
        };

    [Fact]
    public async Task AuthorizeCommand_WhenSuccess_SavesTokensAndNavigatesToMain()
    {
        var refreshToken = CreateRefreshToken("refresh-token");
        var tokens = new TokensInfo("access-token", refreshToken);

        _authApiClientMock
            .Setup(x => x.LoginAsync(It.IsAny<LoginForm>()))
            .ReturnsAsync(Result<TokensInfo>.Success(tokens));

        var vm = CreateViewModel();
        vm.Login = "login";
        vm.Password = "password";

        await ((AsyncRelayCommand)vm.AuthorizeCommand).ExecuteAsync(null);

        Assert.False(vm.HasError);
        _secureTokenStorageMock.VerifySet(x => x.AccessToken = "access-token", Times.Once);
        _secureTokenStorageMock.VerifySet(
            x => x.RefreshToken = It.Is<UserRefreshToken>(t => t.Token == "refresh-token"),
            Times.Once
        );
        _navigationServiceMock.Verify(x => x.NavigateTo<MainWindowViewModel>(), Times.Once);
    }

    [Fact]
    public async Task AuthorizeCommand_WhenFailed_SetsErrorAndDoesNotNavigate()
    {
        _authApiClientMock
            .Setup(x => x.LoginAsync(It.IsAny<LoginForm>()))
            .ReturnsAsync(Result<TokensInfo>.Failure(AuthErrors.InvalidCredentials));

        var vm = CreateViewModel();
        vm.Login = "login";
        vm.Password = "password";

        await ((AsyncRelayCommand)vm.AuthorizeCommand).ExecuteAsync(null);

        Assert.True(vm.HasError);
        Assert.Equal(AuthErrors.InvalidCredentials, vm.ErrorMessage);
        _navigationServiceMock.Verify(x => x.NavigateTo<MainWindowViewModel>(), Times.Never);
        _secureTokenStorageMock.VerifySet(x => x.AccessToken = It.IsAny<string>(), Times.Never);
        _secureTokenStorageMock.VerifySet(x => x.RefreshToken = It.IsAny<UserRefreshToken>(), Times.Never);
    }

    [Fact]
    public async Task AuthorizeCommand_WhenException_ShowsErrorDialog()
    {
        _authApiClientMock
            .Setup(x => x.LoginAsync(It.IsAny<LoginForm>()))
            .ThrowsAsync(new Exception("boom"));

        var vm = CreateViewModel();
        vm.Login = "login";
        vm.Password = "password";

        await ((AsyncRelayCommand)vm.AuthorizeCommand).ExecuteAsync(null);

        _dialogServiceMock.Verify(
            x => x.ShowErrorDialogAsync(DialogTitles.Error, CommonErrors.UnexpectedError),
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
            x => x.ShowDialogAsync<RecoveryCodeDialogView, RecoveryCodeDialogViewModel>(It.IsAny<PasswordRecoveryData>()),
            Times.Once
        );
    }
}