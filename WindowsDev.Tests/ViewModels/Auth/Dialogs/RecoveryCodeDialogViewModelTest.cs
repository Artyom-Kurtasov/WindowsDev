using Moq;
using WindowsDev.Business.Repositories.Interfaces;
using WindowsDev.Business.Services.PasswordManager.PasswordRecovery.Interfaces;
using WindowsDev.Infrastructure;
using WindowsDev.ViewModels.Auth.Dialogs;

namespace WindowsDev.Tests.ViewModels.Authorization.Dialogs
{
    public class RecoveryCodeDialogViewModelTest
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IPasswordRecoveryService> _passwordRecoveryServiceMock;

        private bool _closeRequested;

        public RecoveryCodeDialogViewModelTest()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _passwordRecoveryServiceMock = new Mock<IPasswordRecoveryService>();
        }

        private RecoveryCodeDialogViewModel CreateViewModel()
        {
            return new RecoveryCodeDialogViewModel(
                _userRepositoryMock.Object,
                _passwordRecoveryServiceMock.Object);
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
        public void NextStepCommand_CannotExecute_Initially()
        {
            var vm = CreateViewModel();

            Assert.False(vm.NextStepCommand.CanExecute(null));
        }

        [Fact]
        public async Task CancelCommand_RaisesCloseRequested()
        {
            var vm = CreateViewModel();

            SetupEvents(vm);

            await ((AsyncRelayCommand)vm.CancelCommand)
                .ExecuteAsync(null);

            Assert.True(_closeRequested);
        }
    }
}