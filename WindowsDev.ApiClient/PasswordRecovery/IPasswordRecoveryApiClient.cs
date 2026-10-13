using WindowsDev.Application.Primitives;
using WindowsDev.Contracts.Api.PasswordRecovery;

namespace WindowsDev.ApiClient.PasswordRecovery;

public interface IPasswordRecoveryApiClient
{
    Task<Result<VerifyRecoveryCodeResponse>> IsRecoveryCodeCorrectAsync(
        VerifyRecoveryCodeRequest request,
        CancellationToken cancellationToken
    );

    Task<Result<ResetPasswordResponse>> ChangePasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken
    );

    Task<IsUserExistResponse> IsUserExistAsync(
        IsUserExistRequest request,
        CancellationToken cancellationToken
    );
}
