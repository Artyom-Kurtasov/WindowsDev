using WindowsDev.Application.Primitives;
using WindowsDev.Contracts.Api.Profiles;

namespace WindowsDev.ApiClient.ProfileClient;

public interface IProfileApiClient
{
    Task<Result<ChangePasswordResponse>> ChangePasswordAsync(
        ChangePasswordRequest request,
        CancellationToken cancellationToken
    );

    Task<Result<ChangeUsernameResponse>> ChangeUsernameAsync(
        ChangeUsernameRequest request,
        CancellationToken cancellationToken
    );

    Task<Result<GetProfileResponse>> GetAsync(CancellationToken cancellationToken);
}
