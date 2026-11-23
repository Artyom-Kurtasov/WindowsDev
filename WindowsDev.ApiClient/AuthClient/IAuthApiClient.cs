using WindowsDev.Application.Primitives;
using WindowsDev.Contracts.Api.Identity;
using WindowsDev.Contracts.UI.Identity;

namespace WindowsDev.ApiClient.AuthClient;

public interface IAuthApiClient
{
    Task<Result<RegisterInfo>> RegisterAsync(
        RegisterForm request,
        CancellationToken cancellationToken = default
    );

    Task<Result<TokensInfo>> LoginAsync(
        LoginForm request,
        CancellationToken cancellationToken = default
    );

    Task<Result<TokensInfo>> RefreshAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<AvailabilityInfo>> IsUsernameAvailableAsync(
        UsernameAvailabilityForm form,
        CancellationToken cancellationToken = default
    );

    Task<Result<AvailabilityInfo>> IsLoginAvailableAsync(
        LoginAvailabilityForm form,
        CancellationToken cancellationToken = default
    );
}
