using WindowsDev.Api.DTO.Request.AuthController;
using WindowsDev.Api.DTO.Response.AuthController;
using WindowsDev.Application.Primitives;

namespace WindowsDev.ApiClients.AuthClient;

internal interface IAuthApiClient
{
    Task<Result<UserRegisterResponse>> RegisterAsync(
        UserRegisterRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<UserLoginResponse>> LoginAsync(
        UserLoginRequest request,
        CancellationToken cancellationToken = default
    );
}
