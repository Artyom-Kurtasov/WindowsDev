using WindowsDev.Api.DTO.Request;
using WindowsDev.ApiClients.PasswordRecoveryClient;
using WindowsDev.Application.Common.Utils.DebounceService;

namespace WindowsDev.ViewModels.Authorization.Dialogs.RecoverySteps;

internal class SecondStepViewModel : ViewModelBase
{
    private readonly PasswordRecoveryData _passwordRecoveryData;
    private readonly IPasswordRecoveryApiClient _passwordRecoveryApiClient;
    private readonly IDebounceService _debounceService;

    private const int DebounceDelayMilliseconds = 500;

    public SecondStepViewModel(
        PasswordRecoveryData passwordRecoveryData,
        IPasswordRecoveryApiClient passwordRecoveryApiClient,
        IDebounceService debounceService
    )
    {
        _passwordRecoveryData = passwordRecoveryData;
        _passwordRecoveryApiClient = passwordRecoveryApiClient;
        _debounceService = debounceService;
    }

    public string RecoveryCode
    {
        get => _passwordRecoveryData.RecoveryCode;
        set
        {
            if (_passwordRecoveryData.RecoveryCode == value)
                return;

            _passwordRecoveryData.RecoveryCode = value;

            OnPropertyChanged();

            _ = CheckRecoveryCodeAsync();
        }
    }

    public bool IsRecoveryCodeCorrect
    {
        get => _passwordRecoveryData.IsRecoveryCodeCorrect;
        private set
        {
            if (_passwordRecoveryData.IsRecoveryCodeCorrect == value)
                return;

            _passwordRecoveryData.IsRecoveryCodeCorrect = value;

            OnPropertyChanged();
        }
    }

    private async Task CheckRecoveryCodeAsync()
    {
        await _debounceService.DebounceAsync(
            async () =>
            {
                if (!int.TryParse(RecoveryCode, out int code))
                {
                    IsRecoveryCodeCorrect = false;
                    return;
                }

                var result = await _passwordRecoveryApiClient.IsRecoveryCodeCorrectAsync(
                    new VerifyPasswordRecoveryCodeRequest
                    {
                        recoveryCode = code,
                        login = _passwordRecoveryData.Login,
                    },
                    CancellationToken.None
                );

                IsRecoveryCodeCorrect = result.IsSuccess;
            },
            TimeSpan.FromMilliseconds(DebounceDelayMilliseconds)
        );
    }
}