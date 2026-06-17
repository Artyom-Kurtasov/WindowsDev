using MahApps.Metro.Controls.Dialogs;
using System.Windows.Input;
using WindowsDev.Business.Repositories.Interfaces;
using WindowsDev.Business.Services.PasswordManager.PasswordRecovery.Interfaces;
using WindowsDev.Business.Services.Registration.Validation;
using WindowsDev.Dialogs.Interfaces;
using WindowsDev.Domain.PasswordRecoveryModels;
using WindowsDev.Infrastructure;
using WindowsDev.ViewModels.Auth.Dialogs.RecoverySteps;

namespace WindowsDev.ViewModels.Auth.Dialogs
{
    public class RecoveryCodeDialogViewModel : ViewModelBase, IDialogViewModel
    {
        private readonly IPasswordRecoveryService _passwordRecoveryService;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly PasswordRecoveryData _passwordRecoveryData;
        private readonly ThirdStepViewModel _thirdStepVM;

        private readonly List<object> _steps;

        public RecoveryCodeDialogViewModel(IUserRepository userRepository,
            IPasswordRecoveryService passwordRecoveryService)
        {
            _passwordRecoveryService = passwordRecoveryService;
            _passwordRecoveryData = new PasswordRecoveryData();

            // ThirdStepViewModel is reused across step transitions,
            // so it's created once and stored in a field
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

            // Any change in shared data may affect step validity,
            // so we force all commands to re-evaluate CanExecute
            _passwordRecoveryData.PropertyChanged += (_, _) =>
            {
                ((RelayCommand)NextStepCommand).RaiseCanExecuteChanged();
                ((RelayCommand)PrevStepCommand).RaiseCanExecuteChanged();
                ((AsyncRelayCommand)ChangePasswordCommand).RaiseCanExecuteChanged();

                OnPropertyChanged(nameof(NextStepToolTip));
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

        public event Func<Task>? CloseRequested;
        public event Func<Task>? Completed;

        // Dynamic tooltip based on why the "Next" button is disabled
        public string NextStepToolTip => CurrentStep switch
        {
            0 when !_passwordRecoveryData.IsUserExist =>
                Translate("Tooltip_UserNotFound"),

            1 when !_passwordRecoveryData.IsRecoveryCodeCorrect =>
                Translate("Tooltip_InvalidRecoveryCode"),

            _ => Translate("Tooltip_NextStep")
        };

        private int _currentStep;
        public int CurrentStep
        {
            get => _currentStep;
            set
            {
                _currentStep = value;

                OnPropertyChanged(nameof(CurrentStep));
                OnPropertyChanged(nameof(NextStepToolTip));

                // Step change may enable/disable navigation buttons
                ((RelayCommand)NextStepCommand).RaiseCanExecuteChanged();
                ((RelayCommand)PrevStepCommand).RaiseCanExecuteChanged();
                ((AsyncRelayCommand)ChangePasswordCommand).RaiseCanExecuteChanged();
            }
        }

        public bool CanBackStep() => CurrentStep > 0;

        // Each step has its own validation rule for proceeding forward
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
            try
            {
                // After password change, a new recovery code is generated
                // and must be shown immediately. The old one becomes invalid
                int recoveryCode = await _passwordRecoveryService.ChangePasswordAsync(
                    _passwordRecoveryData.Login!,
                    _passwordRecoveryData.NewPassword!);

                await _dialogCoordinator.ShowMessageAsync(this,
                    Translate("Information_Title"),
                    $"{Translate("RecoveryKeyMessage")}\n\n {recoveryCode}",
                    MessageDialogStyle.Affirmative);

                await CancelAsync();
            }
            catch (Exception ex)
            {
                await _dialogCoordinator.ShowMessageAsync(this,
                    Translate("Warning_Title"),
                    Translate(ex.Message),
                    MessageDialogStyle.Affirmative);
            }
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