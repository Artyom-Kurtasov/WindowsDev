using System.Windows.Input;
using WindowsDev.Business.Repositories.Interfaces;
using WindowsDev.Business.Services.PasswordManager.PasswordRecovery;
using WindowsDev.Business.Services.Registration.Validation;
using WindowsDev.Dialogs.Interfaces;
using WindowsDev.Domain.PasswordRecoveryModels;
using WindowsDev.Infrastructure;
using WindowsDev.ViewModels.Auth.Dialogs.RecoverySteps;

namespace WindowsDev.ViewModels.Auth.Dialogs
{
    public class RecoveryCodeDialogViewModel : ViewModelBase, IDialogViewModel
    {
        private readonly PasswordRecoveryData _passwordRecoveryData;
        private readonly ThirdStepViewModel _thirdStepVM;
        private readonly IPasswordRecoveryService _passwordRecoveryService;

        private readonly List<object> _steps;

        public RecoveryCodeDialogViewModel(
            IUserRepository userRepository,
            IPasswordRecoveryService passwordRecoveryService)
        {
            _passwordRecoveryService = passwordRecoveryService;
            _passwordRecoveryData = new PasswordRecoveryData();

            _thirdStepVM = new ThirdStepViewModel(_passwordRecoveryData);

            _steps = new List<object>
            {
                new FirstStepViewModel(_passwordRecoveryData, userRepository),
                new SecondStepViewModel(_passwordRecoveryData, passwordRecoveryService),
                _thirdStepVM
            };

            NextStepCommand = new RelayCommand(NextStep, CanNextStep);
            PrevStepCommand = new RelayCommand(PrevStep, CanBackStep);
            ChangePasswordCommand = new AsyncRelayCommand(ChangePasswordAsync, CanNextStep);
            CancelCommand = new AsyncRelayCommand(CancelAsync);

            _passwordRecoveryData.PropertyChanged += (_, _) =>
            {
                ((RelayCommand)NextStepCommand).RaiseCanExecuteChanged();
                ((RelayCommand)PrevStepCommand).RaiseCanExecuteChanged();
                ((AsyncRelayCommand)ChangePasswordCommand).RaiseCanExecuteChanged();
            };

            CurrentStep = 0;
            CurrentStepView = _steps[0];
        }

        public ICommand CancelCommand { get; }
        public ICommand ChangePasswordCommand { get; }
        public ICommand NextStepCommand { get; }
        public ICommand PrevStepCommand { get; }

        private object _currentStepView = null!;

        public object CurrentStepView
        {
            get => _currentStepView;
            set
            {
                _currentStepView = value;
                OnPropertyChanged(nameof(CurrentStepView));
            }
        }

        private int _currentStep;

        public event Func<Task>? CloseRequested;
        public event Func<Task>? Completed;

        public int CurrentStep
        {
            get => _currentStep;
            set
            {
                _currentStep = value;

                OnPropertyChanged(nameof(CurrentStep));

                ((RelayCommand)NextStepCommand).RaiseCanExecuteChanged();
                ((RelayCommand)PrevStepCommand).RaiseCanExecuteChanged();
                ((AsyncRelayCommand)ChangePasswordCommand).RaiseCanExecuteChanged();
            }
        }

        public bool CanBackStep() => CurrentStep > 0;

        public bool CanNextStep() => CurrentStep switch
        {
            0 => _passwordRecoveryData.IsUserExist,
            1 => _passwordRecoveryData.IsRecoveryCodeCorrect,
            2 => PasswordValidator.IsValid(_thirdStepVM.NewPassword)
                 && _thirdStepVM.NewPassword == _thirdStepVM.ConfirmPassword,
            _ => false
        };

        private void NextStep()
        {
            if (CurrentStep < _steps.Count - 1)
            {
                CurrentStep++;
                CurrentStepView = _steps[CurrentStep];
            }
        }

        private void PrevStep()
        {
            if (CurrentStep > 0)
            {
                CurrentStep--;
                CurrentStepView = _steps[CurrentStep];
            }
        }

        private async Task ChangePasswordAsync()
        {
            await _passwordRecoveryService.ChangePasswordAsync(
                _passwordRecoveryData.Login!,
                _passwordRecoveryData.NewPassword!);

            await CancelAsync();
        }

        private async Task CancelAsync()
        {
            if (CloseRequested != null)
            {
                await CloseRequested.Invoke();
            }
        }
    }
}