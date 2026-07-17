using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using Moq;
using WindowsDev.Business.Primitives;
using WindowsDev.Business.Services.Localization.Interfaces;
using WindowsDev.Business.Services.PasswordManager.PasswordRecovery.Interfaces;
using WindowsDev.Domain;
using WindowsDev.Domain.DialogsMessages.Errors;
using WindowsDev.Domain.DialogsMessages.Informations;
using WindowsDev.Domain.PasswordRecoveryModels;
using WindowsDev.Infrastructure;
using WindowsDev.ViewModels.Auth.Dialogs;
using WindowsDev.ViewModels.Auth.Dialogs.Factories;

namespace WindowsDev.Tests.ViewModels.Authorization.Dialogs
{
    public class RecoveryCodeDialogViewModelTest
    {
        private readonly Mock<IPasswordRecoveryService> _passwordRecoveryServiceMock;
        private readonly Mock<IDialogCoordinator> _dialogCoordinatorMock;
        private readonly Mock<ILogger<RecoveryCodeDialogViewModel>> _loggerMock;
        private readonly Mock<ILanguageChanger> _languageChangerMock;
        private readonly Mock<IRecoveryStepsFactory> _recoveryStepsFactoryMock;

        private readonly PasswordRecoveryData _passwordRecoveryData;

        private bool _closeRequested;

        public RecoveryCodeDialogViewModelTest()
        {
            _passwordRecoveryServiceMock = new Mock<IPasswordRecoveryService>();
            _dialogCoordinatorMock = new Mock<IDialogCoordinator>();
            _loggerMock = new Mock<ILogger<RecoveryCodeDialogViewModel>>();
            _languageChangerMock = new Mock<ILanguageChanger>();
            _recoveryStepsFactoryMock = new Mock<IRecoveryStepsFactory>();

            _passwordRecoveryData = new PasswordRecoveryData();

            _languageChangerMock
                .Setup(x => x.Translate(It.IsAny<string>()))
                .Returns((string key) => key);

            _recoveryStepsFactoryMock
                .Setup(x => x.CreateSteps())
                .Returns(new List<object> { new object(), new object(), new object() });
        }

        private RecoveryCodeDialogViewModel CreateViewModel()
        {
            return new RecoveryCodeDialogViewModel(
                _passwordRecoveryServiceMock.Object,
                _dialogCoordinatorMock.Object,
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
        public void NextStepCommand_WhenCurrentStepGreaterZenStepsCount_DoesNothing()
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

            _passwordRecoveryServiceMock
                .Setup(x => x.ChangePasswordAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(Result<int>.Success(123456));

            Assert.True(vm.ChangePasswordCommand.CanExecute(null));

            await ((AsyncRelayCommand)vm.ChangePasswordCommand).ExecuteAsync(null);

            _dialogCoordinatorMock.Verify(
                x =>
                    x.ShowMessageAsync(
                        vm,
                        DialogTitles.Information,
                        $"{PasswordRecoveryInformations.RecoveryCodeMessage}\n\n123456",
                        MessageDialogStyle.Affirmative
                    ),
                Times.Once
            );

            Assert.True(_closeRequested);
        }

        [Fact]
        public async Task ChangePassword_WhenExceptionOccurs_ShowsErrorDialog()
        {
            var vm = CreateViewModel();

            _passwordRecoveryData.Login = "test";
            _passwordRecoveryData.NewPassword = "Password123!";
            _passwordRecoveryData.ConfirmPassword = "Password123!";

            vm.CurrentStep = 2;

            _passwordRecoveryServiceMock
                .Setup(x => x.ChangePasswordAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception());

            Assert.True(vm.ChangePasswordCommand.CanExecute(null));

            await ((AsyncRelayCommand)vm.ChangePasswordCommand).ExecuteAsync(null);

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
    }
}
