using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using Moq;
using WindowsDev.ApiClient.PasswordRecovery;
using WindowsDev.Application.Common.Localization;
using WindowsDev.Application.Primitives;
using WindowsDev.Command;
using WindowsDev.Contracts.UI.PasswordRecovery;
using WindowsDev.Domain.Messages;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;
using WindowsDev.Domain.Messages.DialogsMessages.Informations;
using WindowsDev.Factories;
using WindowsDev.Services.Dialogs;
using WindowsDev.ViewModels.Authorization.Dialogs;

namespace WindowsDev.Tests.ViewModels.Authorization.Dialogs;

public class RecoveryCodeDialogViewModelTest
{
    private readonly Mock<IPasswordRecoveryApiClient> _passwordRecoveryApiMock;
    private readonly Mock<IDialogService> _dialogServiceMock;
    private readonly Mock<ILogger<RecoveryCodeDialogViewModel>> _loggerMock;
    private readonly Mock<ILanguageChanger> _languageChangerMock;
    private readonly Mock<IRecoveryStepsFactory> _recoveryStepsFactoryMock;

    private readonly PasswordRecoveryData _passwordRecoveryData;

    private bool _closeRequested;

    public RecoveryCodeDialogViewModelTest()
    {
        _passwordRecoveryApiMock = new Mock<IPasswordRecoveryApiClient>();
        _dialogServiceMock = new Mock<IDialogService>();
        _loggerMock = new Mock<ILogger<RecoveryCodeDialogViewModel>>();
        _languageChangerMock = new Mock<ILanguageChanger>();
        _recoveryStepsFactoryMock = new Mock<IRecoveryStepsFactory>();

        _passwordRecoveryData = new PasswordRecoveryData();

        _languageChangerMock
            .Setup(x => x.Translate(It.IsAny<string>()))
            .Returns((string key) => key);

        _recoveryStepsFactoryMock
            .Setup(x => x.CreateSteps(It.IsAny<PasswordRecoveryData>()))
            .Returns(new List<object> { new object(), new object(), new object() });
    }

    private RecoveryCodeDialogViewModel CreateViewModel()
    {
        return new RecoveryCodeDialogViewModel(
            _passwordRecoveryApiMock.Object,
            _dialogServiceMock.Object,
            _loggerMock.Object,
            _languageChangerMock.Object,
            _recoveryStepsFactoryMock.Object,
            _passwordRecoveryData
        );
    }

    private void SetupEvents(RecoveryCodeDialogViewModel vm)
    {
        _closeRequested = false;

        vm.CloseRequested += () =>
        {
            _closeRequested = true;
            return Task.CompletedTask;
        };
    }

    [Fact]
    public void Constructor_SetsFirstStep()
    {
        var vm = CreateViewModel();

        Assert.Equal(0, vm.CurrentStep);
        Assert.NotNull(vm.CurrentStepView);
    }

    [Fact]
    public void Constructor_CallsCreateStepsWithPasswordRecoveryData()
    {
        var vm = CreateViewModel();

        _recoveryStepsFactoryMock.Verify(
            x => x.CreateSteps(_passwordRecoveryData),
            Times.Once
        );
    }

    [Fact]
    public void CanBackStep_WhenCurrentStepZero_ReturnsFalse()
    {
        var vm = CreateViewModel();

        Assert.False(vm.CanBackStep());
    }

    [Fact]
    public void CanBackStep_WhenCurrentStepGreaterThanZero_ReturnsTrue()
    {
        var vm = CreateViewModel();

        vm.CurrentStep = 1;

        Assert.True(vm.CanBackStep());
    }

    [Fact]
    public void PrevStep_WhenCurrentStepZero_DoesNothing()
    {
        var vm = CreateViewModel();

        ((RelayCommand)vm.PrevStepCommand).Execute(null);

        Assert.Equal(0, vm.CurrentStep);
    }

    [Fact]
    public void PrevStep_WhenCurrentStepGreaterThanZero_GoesBack()
    {
        var vm = CreateViewModel();

        vm.CurrentStep = 1;

        ((RelayCommand)vm.PrevStepCommand).Execute(null);

        Assert.Equal(0, vm.CurrentStep);
    }

    [Fact]
    public void NextStepCommand_WhenCurrentStepGreaterThanStepsCount_DoesNothing()
    {
        var vm = CreateViewModel();

        vm.CurrentStep = 2;

        ((RelayCommand)vm.NextStepCommand).Execute(null);

        Assert.Equal(2, vm.CurrentStep);
    }

    [Fact]
    public void NextStep_WhenCurrentStepZero_GoesNext()
    {
        var vm = CreateViewModel();

        ((RelayCommand)vm.NextStepCommand).Execute(null);

        Assert.Equal(1, vm.CurrentStep);
    }

    [Fact]
    public void CanNextStep_WhenStepZeroAndUserNotExist_ReturnsFalse()
    {
        _passwordRecoveryData.IsUserExist = false;
        var vm = CreateViewModel();

        vm.CurrentStep = 0;

        Assert.False(vm.CanNextStep());
    }

    [Fact]
    public void CanNextStep_WhenStepZeroAndUserExists_ReturnsTrue()
    {
        _passwordRecoveryData.IsUserExist = true;
        var vm = CreateViewModel();

        vm.CurrentStep = 0;

        Assert.True(vm.CanNextStep());
    }

    [Fact]
    public void CanNextStep_WhenStepTwoAndPasswordsMatch_ReturnsTrue()
    {
        var vm = CreateViewModel();

        _passwordRecoveryData.NewPassword = "Password123!";
        _passwordRecoveryData.ConfirmPassword = "Password123!";

        vm.CurrentStep = 2;

        Assert.True(vm.CanNextStep());
    }

    [Fact]
    public void CanNextStep_WhenStepTwoAndPasswordsDoNotMatch_ReturnsFalse()
    {
        var vm = CreateViewModel();

        _passwordRecoveryData.NewPassword = "Password123!";
        _passwordRecoveryData.ConfirmPassword = "Different123!";

        vm.CurrentStep = 2;

        Assert.False(vm.CanNextStep());
    }

    [Fact]
    public async Task CancelCommand_RaisesCloseRequested()
    {
        var vm = CreateViewModel();

        SetupEvents(vm);

        await ((AsyncRelayCommand)vm.CancelCommand).ExecuteAsync(null);

        Assert.True(_closeRequested);
    }

    [Fact]
    public async Task ChangePassword_WhenSuccess_ShowsInformationDialogAndCloses()
    {
        var vm = CreateViewModel();

        SetupEvents(vm);

        _passwordRecoveryData.Login = "test";
        _passwordRecoveryData.NewPassword = "Password123!";
        _passwordRecoveryData.ConfirmPassword = "Password123!";

        vm.CurrentStep = 2;

        var recoveryCode = new ResetPasswordInfo(123456);

        _passwordRecoveryApiMock
            .Setup(x => x.ChangePasswordAsync(
                It.IsAny<ResetPasswordForm>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ResetPasswordInfo>.Success(recoveryCode));

        Assert.True(vm.ChangePasswordCommand.CanExecute(null));

        await ((AsyncRelayCommand)vm.ChangePasswordCommand).ExecuteAsync(null);

        _dialogServiceMock.Verify(
            x => x.ShowMessageAsync(
                DialogTitles.Information,
                $"{PasswordRecoveryInformations.RecoveryCodeMessage}\n\n123456",
                MessageDialogStyle.Affirmative
            ),
            Times.Once
        );

        _dialogServiceMock.Verify(
            x => x.ShowMessageAsync(
                DialogTitles.Error,
                It.IsAny<string>(),
                It.IsAny<MessageDialogStyle>()
            ),
            Times.Never
        );

        Assert.True(_closeRequested);
    }

    [Fact]
    public async Task ChangePassword_WhenApiReturnsFailure_ShowsErrorDialogAndDoesNotClose()
    {
        var vm = CreateViewModel();

        SetupEvents(vm);

        _passwordRecoveryData.Login = "test";
        _passwordRecoveryData.NewPassword = "Password123!";
        _passwordRecoveryData.ConfirmPassword = "Password123!";

        vm.CurrentStep = 2;

        _passwordRecoveryApiMock
            .Setup(x => x.ChangePasswordAsync(
                It.IsAny<ResetPasswordForm>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ResetPasswordInfo>.Failure(CommonErrors.UnexpectedError));

        await ((AsyncRelayCommand)vm.ChangePasswordCommand).ExecuteAsync(null);

        _dialogServiceMock.Verify(
            x => x.ShowMessageAsync(
                DialogTitles.Error,
                CommonErrors.UnexpectedError,
                MessageDialogStyle.Affirmative
            ),
            Times.Once
        );

        _dialogServiceMock.Verify(
            x => x.ShowMessageAsync(
                DialogTitles.Information,
                It.IsAny<string>(),
                It.IsAny<MessageDialogStyle>()
            ),
            Times.Never
        );

        Assert.False(_closeRequested);
    }

    [Fact]
    public async Task ChangePassword_WhenExceptionOccurs_ShowsErrorDialog()
    {
        var vm = CreateViewModel();

        _passwordRecoveryData.Login = "test";
        _passwordRecoveryData.NewPassword = "Password123!";
        _passwordRecoveryData.ConfirmPassword = "Password123!";

        vm.CurrentStep = 2;

        _passwordRecoveryApiMock
            .Setup(x => x.ChangePasswordAsync(
                It.IsAny<ResetPasswordForm>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("boom"));

        Assert.True(vm.ChangePasswordCommand.CanExecute(null));

        await ((AsyncRelayCommand)vm.ChangePasswordCommand).ExecuteAsync(null);

        _dialogServiceMock.Verify(
            x => x.ShowMessageAsync(
                DialogTitles.Error,
                CommonErrors.UnexpectedError,
                MessageDialogStyle.Affirmative
            ),
            Times.Once
        );

        _dialogServiceMock.Verify(
            x => x.ShowMessageAsync(
                DialogTitles.Information,
                It.IsAny<string>(),
                It.IsAny<MessageDialogStyle>()
            ),
            Times.Never
        );
    }
}