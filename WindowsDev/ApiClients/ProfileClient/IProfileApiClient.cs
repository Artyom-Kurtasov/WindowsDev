using WindowsDev.Api.DTO.Request.ProfilesController;
using WindowsDev.Api.DTO.Response.ProfilesController;
using WindowsDev.Application.Primitives;

namespace WindowsDev.ApiClients.ProfileClient;

public interface IProfileApiClient
{
    Task<Result<ProfileChangePasswordResponse>> ChangePasswordAsync(
        ProfileChangePasswordRequest request,
        CancellationToken cancellationToken
    );

    Task<Result<UsernameChangeResponse>> ChangeUsernameAsync(
        UsernameChangeRequest request,
        CancellationToken cancellationToken
    );

    Task<Result<GetAsyncResponse>> GetAsync(CancellationToken cancellationToken);
}
