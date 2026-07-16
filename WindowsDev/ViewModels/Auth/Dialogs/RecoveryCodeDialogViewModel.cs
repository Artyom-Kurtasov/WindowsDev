using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using System.Windows.Input;
using WindowsDev.Business.Services.Localization.Interfaces;
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
using WindowsDev.ViewModels.Auth.Dialogs.Factories;

using WindowsDev.ViewModels.Auth.Dialogs.RecoverySteps;

namespace WindowsDev.ViewModels.Auth.Dialogs
{
    public class RecoveryCodeDialogViewModel : LocalizedViewModelBase, IDialogViewModel
    {
        private readonly IPasswordRecoveryService _passwordRecoveryService;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly PasswordRecoveryData _passwordRecoveryData;
        private readonly ILogger<RecoveryCodeDialogViewModel> _logger;
        private readonly IRecoveryStepsFactory _recoveryStepsFactory;

        private readonly IReadOnlyList<object> _steps;

        public RecoveryCodeDialogViewModel(IPasswordRecoveryService passwordRecoveryService,
            IDialogCoordinator dialogCoordinator,
            ILogger<RecoveryCodeDialogViewModel> logger,
            ILanguageChanger languageChanger,
            IRecoveryStepsFactory recoveryStepsFactory,
            PasswordRecoveryData passwordRecoveryData) : base(languageChanger)
        {
            _passwordRecoveryService = passwordRecoveryService;
            _dialogCoordinator = dialogCoordinator;
            _logger = logger;
            _recoveryStepsFactory = recoveryStepsFactory;
            _passwordRecoveryData = passwordRecoveryData;

            _steps = _recoveryStepsFactory.CreateSteps();

            NextStepCommand = new RelayCommand(NextStep, CanNextStep);
            PrevStepCommand = new RelayCommand(PrevStep, CanBackStep);
            ChangePasswordCommand = new AsyncRelayCommand(ChangePasswordAsync, CanNextStep);
            CancelCommand = new AsyncRelayCommand(CancelAsync);


            _passwordRecoveryData.PropertyChanged += PasswordRecoveryDataChanged;


            CurrentStep = 0;
            CurrentStepView = _steps[0];
        }

        public ICommand CancelCommand { get; }
        public ICommand ChangePasswordCommand { get; }
        public ICommand NextStepCommand { get; }
        public ICommand PrevStepCommand { get; }

        public event Func<Task>? CloseRequested;
        public event Func<Task>? Completed;

        private object _currentStepView = null!;

        public object CurrentStepView
        {
            get => _currentStepView;
            set
            {
                if (_currentStepView == value)
                    return;

                _currentStepView = value;
                OnPropertyChanged();
            }
        }

        private int _currentStep;

        public int CurrentStep
        {
            get => _currentStep;
            set
            {
                if (_currentStep == value)
                    return;

                _currentStep = value;

                OnPropertyChanged();

                UpdateCommands();
            }
        }

        public string NextStepToolTip => CurrentStep switch
        {
            0 when !_passwordRecoveryData.IsUserExist =>
                Translate(PasswordRecoveryTooltips.UserNotFound),

            1 when !_passwordRecoveryData.IsRecoveryCodeCorrect =>
                Translate(PasswordRecoveryTooltips.InvalidRecoveryCode),

            _ =>
                Translate(PasswordRecoveryTooltips.NextStep)
        };

        public bool CanBackStep() =>
            CurrentStep > 0;

        public bool CanNextStep() =>
            CurrentStep switch
            {
                0 => _passwordRecoveryData.IsUserExist,

                1 => _passwordRecoveryData.IsRecoveryCodeCorrect,

                2 => PasswordValidator.IsValid(_passwordRecoveryData.NewPassword) &&
                     _passwordRecoveryData.NewPassword == _passwordRecoveryData.ConfirmPassword,

                _ => false
            };

        private void NextStep()
        {
            if (CurrentStep >= _steps.Count - 1)
                return;

            CurrentStep++;
            CurrentStepView = _steps[CurrentStep];
        }

        private void PrevStep()
        {
            if (CurrentStep <= 0)
                return;

            CurrentStep--;
            CurrentStepView = _steps[CurrentStep];
        }

        private async Task ChangePasswordAsync()
        {
            try
            {
                var result = await _passwordRecoveryService.ChangePasswordAsync(
                    _passwordRecoveryData.Login!,
                    _passwordRecoveryData.NewPassword!);

                await _dialogCoordinator.ShowMessageAsync(
                    this,
                    Translate(DialogTitles.Information),
                    $"{Translate(PasswordRecoveryInformations.RecoveryCodeMessage)}\n\n{result.Value}",
                    MessageDialogStyle.Affirmative);


                await CancelAsync();
            }
            catch (Exception ex)
            {
                RecoveryLogs.PasswordResetFailed(
                    _logger,
                    _passwordRecoveryData.Login ?? "unknown",
                    ex);


                await _dialogCoordinator.ShowMessageAsync(
                    this,
                    Translate(DialogTitles.Error),
                    Translate(CommonErrors.UnexpectedError),
                    MessageDialogStyle.Affirmative);
            }
        }

        private async Task CancelAsync()
        {
            if (CloseRequested != null)
                await CloseRequested.Invoke();
        }

        private void PasswordRecoveryDataChanged(object? sender, EventArgs e)
        {
            UpdateCommands();

            OnPropertyChanged(nameof(NextStepToolTip));
        }

        private void UpdateCommands()
        {
            ((RelayCommand)NextStepCommand).RaiseCanExecuteChanged();
            ((RelayCommand)PrevStepCommand).RaiseCanExecuteChanged();
            ((AsyncRelayCommand)ChangePasswordCommand).RaiseCanExecuteChanged();
        }
    }
}