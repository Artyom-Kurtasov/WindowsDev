using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using Moq;
using WindowsDev.ApiClient.AuthClient;
using WindowsDev.Application.Common.DebounceService;
using WindowsDev.Application.Common.Localization;
using WindowsDev.Application.Identity.Cryptography;
using WindowsDev.Application.Primitives;
using WindowsDev.Command;
using WindowsDev.Contracts.UI.Identity;
using WindowsDev.Domain.Entities;
using WindowsDev.Domain.Messages;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;
using WindowsDev.Domain.Messages.DialogsMessages.Informations;
using WindowsDev.Services.Dialogs;
using WindowsDev.Services.Navigation;
using WindowsDev.ViewModels.Authorization;
using WindowsDev.ViewModels.Main;
using WindowsDev.ViewModels.Registration;

namespace WindowsDev.Tests.ViewModels.Auth;

public class RegistrationViewModelTest
{
    private readonly Mock<IAuthApiClient> _authApiClientMock;
    private readonly Mock<INavigationService> _navigationServiceMock;
    private readonly Mock<IDialogService> _dialogServiceMock;
    private readonly Mock<ILogger<RegistrationViewModel>> _loggerMock;
    private readonly Mock<IDebounceService> _debounceServiceMock;
    private readonly Mock<ILanguageChanger> _languageChangerMock;
    private readonly Mock<ISecureTokenStorage> _secureTokenStorageMock;

    public RegistrationViewModelTest()
    {
        _authApiClientMock = new Mock<IAuthApiClient>();
        _navigationServiceMock = new Mock<INavigationService>();
        _dialogServiceMock = new Mock<IDialogService>();
        _loggerMock = new Mock<ILogger<RegistrationViewModel>>();
        _debounceServiceMock = new Mock<IDebounceService>();
        _languageChangerMock = new Mock<ILanguageChanger>();
        _secureTokenStorageMock = new Mock<ISecureTokenStorage>();

        _languageChangerMock
            .Setup(x => x.Translate(It.IsAny<string>()))
            .Returns((string key) => key);

        _debounceServiceMock
            .Setup(x => x.DebounceAsync(It.IsAny<Func<Task>>(), It.IsAny<TimeSpan>()))
            .Returns<Func<Task>, TimeSpan>(async (action, _) => await action());

        _authApiClientMock
            .Setup(x => x.IsLoginAvailableAsync(It.IsAny<LoginAvailabilityForm>()))
            .ReturnsAsync(Result<AvailabilityInfo>.Success(new AvailabilityInfo(true)));
        _authApiClientMock
            .Setup(x => x.IsUsernameAvailableAsync(It.IsAny<UsernameAvailabilityForm>()))
            .ReturnsAsync(Result<AvailabilityInfo>.Success(new AvailabilityInfo(true)));
    }

    private RegistrationViewModel CreateViewModel() =>
        new RegistrationViewModel(
            _authApiClientMock.Object,
            _navigationServiceMock.Object,
            _dialogServiceMock.Object,
            _loggerMock.Object,
            _debounceServiceMock.Object,
            _languageChangerMock.Object,
            _secureTokenStorageMock.Object
        );

    private void FillValidData(RegistrationViewModel vm)
    {
        vm.Login = "login";
        vm.Username = "username";
        vm.Password = "Password123!";
        vm.ConfirmPassword = "Password123!";
    }

    [Fact]
    public async Task SignUpCommand_WhenSuccess_ShowsInformationDialogAndNavigates()
    {
        var registerResponse = new RegisterInfo(
            RecoveryCode: 123456,
            JwtToken: "jwt-token",
            RefreshToken: new UserRefreshToken
            {
                Id = 1,
                UserId = 42,
                Token = "refresh-token",
                IsRevoked = false,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
            }
        );

        _authApiClientMock
            .Setup(x => x.RegisterAsync(It.IsAny<RegisterForm>()))
            .ReturnsAsync(Result<RegisterInfo>.Success(registerResponse));

        var vm = CreateViewModel();
        FillValidData(vm);

        await ((AsyncRelayCommand)vm.SignUpCommand).ExecuteAsync(null);

        _dialogServiceMock.Verify(
            x => x.ShowMessageAsync(
                DialogTitles.Information,
                $"{PasswordRecoveryInformations.RecoveryCodeMessage}\n\n123456",
                MessageDialogStyle.Affirmative
            ),
            Times.Once
        );
        _secureTokenStorageMock.VerifySet(x => x.AccessToken = "jwt-token", Times.Once);
        _secureTokenStorageMock.VerifySet(
            x => x.RefreshToken = It.Is<UserRefreshToken>(t => t.Token == "refresh-token"),
            Times.Once
        );
        _navigationServiceMock.Verify(x => x.NavigateTo<MainWindowViewModel>(), Times.Once);
    }

    [Fact]
    public async Task SignUpCommand_WhenRegistrationFails_DoesNotNavigate()
    {
        _authApiClientMock
            .Setup(x => x.RegisterAsync(It.IsAny<RegisterForm>()))
            .ReturnsAsync(Result<RegisterInfo>.Failure(AuthErrors.RegistrationFailed));

        var vm = CreateViewModel();
        FillValidData(vm);

        await ((AsyncRelayCommand)vm.SignUpCommand).ExecuteAsync(null);

        Assert.True(vm.HasError);
        Assert.Equal(AuthErrors.RegistrationFailed, vm.ErrorMessage);
        _navigationServiceMock.Verify(x => x.NavigateTo<MainWindowViewModel>(), Times.Never);
    }

    [Fact]
    public async Task SignUpCommand_WhenException_ShowsErrorDialog()
    {
        _authApiClientMock
            .Setup(x => x.RegisterAsync(It.IsAny<RegisterForm>()))
            .ThrowsAsync(new Exception("boom"));

        var vm = CreateViewModel();
        FillValidData(vm);

        await ((AsyncRelayCommand)vm.SignUpCommand).ExecuteAsync(null);

        _dialogServiceMock.Verify(
            x => x.ShowMessageAsync(
                DialogTitles.Error,
                CommonErrors.UnexpectedError,
                MessageDialogStyle.Affirmative
            ),
            Times.Once
        );
        _navigationServiceMock.Verify(x => x.NavigateTo<MainWindowViewModel>(), Times.Never);
    }

    [Fact]
    public async Task SignUpCommand_WhenPasswordsDoNotMatch_DoesNotRegisterAndShowErrorMessage()
    {
        var vm = CreateViewModel();

        vm.Login = "login";
        vm.Username = "username";
        vm.Password = "Password123!";
        vm.ConfirmPassword = "DifferentPassword123!";

        await ((AsyncRelayCommand)vm.SignUpCommand).ExecuteAsync(null);

        Assert.True(vm.HasError);
        Assert.Equal(AuthErrors.RegistrationFailed, vm.ErrorMessage);
        _authApiClientMock.Verify(
            x => x.RegisterAsync(It.IsAny<RegisterForm>()),
            Times.Never
        );
    }

    [Fact]
    public async Task SignUpCommand_WhenLoginUnavailable_DoesNotRegisterAndShowErrorMessage()
    {
        _authApiClientMock
            .Setup(x => x.IsLoginAvailableAsync(It.IsAny<LoginAvailabilityForm>()))
            .ReturnsAsync(Result<AvailabilityInfo>.Success(new AvailabilityInfo(false)));

        var vm = CreateViewModel();
        FillValidData(vm);

        await ((AsyncRelayCommand)vm.SignUpCommand).ExecuteAsync(null);

        Assert.True(vm.HasError);
        Assert.Equal(AuthErrors.RegistrationFailed, vm.ErrorMessage);
        _authApiClientMock.Verify(
            x => x.RegisterAsync(It.IsAny<RegisterForm>()),
            Times.Never
        );
    }

    [Fact]
    public async Task SignUpCommand_WhenUsernameUnavailable_DoesNotRegisterAndShowErrorMessage()
    {
        _authApiClientMock
            .Setup(x => x.IsUsernameAvailableAsync(It.IsAny<UsernameAvailabilityForm>()))
            .ReturnsAsync(Result<AvailabilityInfo>.Success(new AvailabilityInfo(false)));

        var vm = CreateViewModel();
        FillValidData(vm);

        await ((AsyncRelayCommand)vm.SignUpCommand).ExecuteAsync(null);

        Assert.True(vm.HasError);
        Assert.Equal(AuthErrors.RegistrationFailed, vm.ErrorMessage);
        _authApiClientMock.Verify(
            x => x.RegisterAsync(It.IsAny<RegisterForm>()),
            Times.Never
        );
    }

    [Fact]
    public async Task SwitchToAuthView_NavigatesToAuthorization()
    {
        var vm = CreateViewModel();

        await ((AsyncRelayCommand)vm.SwitchToAuthViewCommand).ExecuteAsync(null);

        _navigationServiceMock.Verify(x => x.NavigateTo<AuthorizationViewModel>(), Times.Once);
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