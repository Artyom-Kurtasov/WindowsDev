using WindowsDev.Application.Primitives;
using WindowsDev.Contracts.UI.Profiles;

namespace WindowsDev.ApiClient.ProfileClient;

public interface IProfileApiClient
{
    Task<Result<ChangePasswordInfo>> ChangePasswordAsync(
        ChangePasswordForm request,
        CancellationToken cancellationToken = default
    );

    Task<Result<ChangeUsernameInfo>> ChangeUsernameAsync(
        ChangeUsernameForm request,
        CancellationToken cancellationToken = default
    );

    Task<Result<ProfileInfo>> GetAsync(CancellationToken cancellationToken = default);
}
