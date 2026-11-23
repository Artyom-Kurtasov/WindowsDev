using WindowsDev.Application.Primitives;
using WindowsDev.Contracts.UI.PasswordRecovery;

namespace WindowsDev.ApiClient.PasswordRecovery;

public interface IPasswordRecoveryApiClient
{
    Task<Result<VerifyRecoveryCodeInfo>> IsRecoveryCodeCorrectAsync(
        VerifyRecoveryCodeForm request,
        CancellationToken cancellationToken
    );

    Task<Result<ResetPasswordInfo>> ChangePasswordAsync(
        ResetPasswordForm request,
        CancellationToken cancellationToken
    );

    Task<Result<IsUserExistInfo>> IsUserExistAsync(
        IsUserExistForm request,
        CancellationToken cancellationToken
    );
}
