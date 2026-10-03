using WindowsDev.Api.DTO.Request;
using WindowsDev.Api.DTO.Response;
using WindowsDev.Application.Primitives;

namespace WindowsDev.ApiClients.PasswordRecoveryClient;

public interface IPasswordRecoveryApiClient
{
    Task<Result<VerifyPasswordRecoveryCodeResponse>> IsRecoveryCodeCorrectAsync(
        VerifyPasswordRecoveryCodeRequest request,
        CancellationToken cancellationToken
    );

    Task<Result<ChangePasswordResponse>> ChangePasswordAsync(
        ChangePasswordRequest request,
        CancellationToken cancellationToken
    );

    Task<IsUserExistResponse> IsUserExistAsync(
        IsUserExistRequest request,
        CancellationToken cancellationToken
    );
}
