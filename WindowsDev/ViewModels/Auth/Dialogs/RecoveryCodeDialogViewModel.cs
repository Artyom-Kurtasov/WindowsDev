using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using System.Windows.Input;
using WindowsDev.Business.Repositories.Interfaces;
using WindowsDev.Business.Services.PasswordManager.PasswordRecovery.Interfaces;
using WindowsDev.Business.Services.Registration.Validation;
using WindowsDev.Dialogs.Interfaces;
using WindowsDev.Domain;
using WindowsDev.Domain.DialogsMessages.Errors;
using WindowsDev.Domain.DialogsMessages.Informations;
using WindowsDev.Domain.DialogsMessages.Tooltips;
using WindowsDev.Domain.PasswordRecoveryModels;
using WindowsDev.Infrastructure;
using WindowsDev.Infrastructure.Logging;
using WindowsDev.ViewModels.Auth.Dialogs.RecoverySteps;

namespace WindowsDev.ViewModels.Auth.Dialogs
{
    public class RecoveryCodeDialogViewModel : ViewModelBase, IDialogViewModel
    {
        private readonly IPasswordRecoveryService _passwordRecoveryService;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly PasswordRecoveryData _passwordRecoveryData;
        private readonly ThirdStepViewModel _thirdStepVM;
        private readonly ILogger<RecoveryCodeDialogViewModel> _logger;

        private readonly List<object> _steps;

        public RecoveryCodeDialogViewModel(IUserRepository userRepository,
            IPasswordRecoveryService passwordRecoveryService,
            ILogger<RecoveryCodeDialogViewModel> logger)
        {
            _passwordRecoveryService = passwordRecoveryService;
            _passwordRecoveryData = new PasswordRecoveryData();
            _logger = logger;

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

        public string NextStepToolTip => CurrentStep switch
        {
            0 when !_passwordRecoveryData.IsUserExist =>
                Translate(AuthTooltips.UserNotFound),

            1 when !_passwordRecoveryData.IsRecoveryCodeCorrect =>
                Translate(AuthTooltips.InvalidRecoveryCode),

            _ => Translate(AuthTooltips.NextStep)
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
            try
            {
                var result = await _passwordRecoveryService.ChangePasswordAsync(
                    _passwordRecoveryData.Login!,
                    _passwordRecoveryData.NewPassword!);

                if (result.IsSuccess)
                {
                    await _dialogCoordinator.ShowMessageAsync(this,
                        Translate(DialogTitles.Information),
                        $"{Translate(PasswordRecoveryInformations.RecoveryCodeMessage)}\n\n{result.Value}",
                        MessageDialogStyle.Affirmative);

                    await CancelAsync();
                }
                else
                {
                    await _dialogCoordinator.ShowMessageAsync(this,
                        Translate(DialogTitles.Warning),
                        Translate(result.Error),
                        MessageDialogStyle.Affirmative);
                }
            }
            catch (Exception ex)
            {
                RecoveryLogs.PasswordResetFailed(_logger,
                    _passwordRecoveryData.Login ?? "unknown", ex);
                await _dialogCoordinator.ShowMessageAsync(this,
                    Translate(DialogTitles.Error),
                    Translate(CommonErrors.UnexpectedError),
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
