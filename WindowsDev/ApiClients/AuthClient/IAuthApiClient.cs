using WindowsDev.Api.DTOs.Request;
using WindowsDev.Api.DTOs.Response;
using WindowsDev.Application.Primitives;

namespace WindowsDev.ApiClients.AuthClient;

internal interface IAuthApiClient
{
    Task<Result<UserRegisterResponse>> RegisterAsync(
        UserRegisterRequest request,
        CancellationToken cancellationToken = default);
}
