using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using System.Windows.Input;
using WindowsDev.ApiClient.PasswordRecovery;
using WindowsDev.Application.Common.Localization;
using WindowsDev.Business.Services.Registration.Validation;
using WindowsDev.Command;
using WindowsDev.Contracts.PasswordRecovery;
using WindowsDev.Domain.Messages;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;
using WindowsDev.Domain.Messages.DialogsMessages.Informations;
using WindowsDev.Domain.Messages.DialogsMessages.Tooltips;
using WindowsDev.Factories;
using WindowsDev.Infrastructure.Logging;
using WindowsDev.Services.Dialogs;

namespace WindowsDev.ViewModels.Authorization.Dialogs;

internal class RecoveryCodeDialogViewModel : LocalizedViewModelBase, IDialogViewModel
{
    private readonly IPasswordRecoveryApiClient _passwordRecoveryApiClient;
    private readonly IDialogService _dialogService;
    private readonly ILogger<RecoveryCodeDialogViewModel> _logger;
    private readonly IRecoveryStepsFactory _recoveryStepsFactory;
    private readonly PasswordRecoveryData _passwordRecoveryData;

    private readonly IReadOnlyList<object> _steps;

    public RecoveryCodeDialogViewModel(
        IPasswordRecoveryApiClient passwordRecoveryApiClient,
        IDialogService dialogService,
        ILogger<RecoveryCodeDialogViewModel> logger,
        ILanguageChanger languageChanger,
        IRecoveryStepsFactory recoveryStepsFactory,
        PasswordRecoveryData passwordRecoveryData
    )
        : base(languageChanger)
    {
        _passwordRecoveryApiClient = passwordRecoveryApiClient;
        _dialogService = dialogService;
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

    public string NextStepToolTip =>
        CurrentStep switch
        {
            0 when !_passwordRecoveryData.IsUserExist => Translate(
                PasswordRecoveryTooltips.UserNotFound
            ),

            1 when !_passwordRecoveryData.IsRecoveryCodeCorrect => Translate(
                PasswordRecoveryTooltips.InvalidRecoveryCode
            ),

            _ => Translate(PasswordRecoveryTooltips.NextStep),
        };

    public bool CanBackStep() => CurrentStep > 0;

    public bool CanNextStep() =>
        CurrentStep switch
        {
            0 => _passwordRecoveryData.IsUserExist,

            1 => _passwordRecoveryData.IsRecoveryCodeCorrect,

            2 => PasswordValidator.IsValid(_passwordRecoveryData.NewPassword)
                && _passwordRecoveryData.NewPassword == _passwordRecoveryData.ConfirmPassword,

            _ => false,
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
            var result = await _passwordRecoveryApiClient.ChangePasswordAsync(
                new ResetPasswordRequest
                {
                    Login = _passwordRecoveryData.Login,
                    Password = _passwordRecoveryData.NewPassword,
                },
                CancellationToken.None
            );

            if (!result.IsSuccess || result.Value is null)
            {
                await _dialogService.ShowMessageAsync(
                    Translate(DialogTitles.Error),
                    Translate(string.IsNullOrWhiteSpace(result.Error)
                        ? CommonErrors.UnexpectedError
                        : result.Error),
                    MessageDialogStyle.Affirmative
                );
                return;
            }

            await _dialogService.ShowMessageAsync(
                Translate(DialogTitles.Information),
                $"{Translate(PasswordRecoveryInformations.RecoveryCodeMessage)}\n\n{result.Value.RecoveryCode}",
                MessageDialogStyle.Affirmative
            );

            await CancelAsync();
        }
        catch (Exception ex)
        {
            RecoveryLogs.PasswordResetFailed(
                _logger,
                _passwordRecoveryData.Login ?? "unknown",
                ex
            );

            await _dialogService.ShowMessageAsync(
                Translate(DialogTitles.Error),
                Translate(CommonErrors.UnexpectedError),
                MessageDialogStyle.Affirmative
            );
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