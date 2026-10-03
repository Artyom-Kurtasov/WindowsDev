using WindowsDev.Api.DTO.Request;
using WindowsDev.ApiClients.PasswordRecoveryClient;
using WindowsDev.Application.Common.Utils.DebounceService;

namespace WindowsDev.ViewModels.Authorization.Dialogs.RecoverySteps;

internal class FirstStepViewModel : ViewModelBase
{
    private readonly PasswordRecoveryData _passwordRecoveryData;
    private readonly IPasswordRecoveryApiClient _passwordRecoveryApiClient;
    private readonly IDebounceService _debounceService;

    private const int DebounceDelayMilliseconds = 300;

    public FirstStepViewModel(
        PasswordRecoveryData passwordRecoveryData,
        IPasswordRecoveryApiClient passwordRecoveryApiClient,
        IDebounceService debounceService
    )
    {
        _passwordRecoveryData = passwordRecoveryData;
        _passwordRecoveryApiClient = passwordRecoveryApiClient;
        _debounceService = debounceService;
    }

    public string Login
    {
        get => _passwordRecoveryData.Login;
        set
        {
            if (_passwordRecoveryData.Login == value)
                return;

            _passwordRecoveryData.Login = value;

            OnPropertyChanged();

            _ = CheckUserAsync();
        }
    }

    public bool IsUserExist
    {
        get => _passwordRecoveryData.IsUserExist;
        private set
        {
            if (_passwordRecoveryData.IsUserExist == value)
                return;

            _passwordRecoveryData.IsUserExist = value;

            OnPropertyChanged();
        }
    }

    private async Task CheckUserAsync()
    {
        await _debounceService.DebounceAsync(
            async () =>
            {
                IsUserExist =
                    !string.IsNullOrWhiteSpace(Login)
                    && (
                        await _passwordRecoveryApiClient.IsUserExistAsync(
                            new IsUserExistRequest { Login = Login },
                            CancellationToken.None
                        )
                    ).IsExist;
            },
            TimeSpan.FromMilliseconds(DebounceDelayMilliseconds)
        );
    }
}