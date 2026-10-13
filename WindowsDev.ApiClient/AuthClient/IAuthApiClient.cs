using WindowsDev.Application.Primitives;
using WindowsDev.Contracts.UI;

namespace WindowsDev.ApiClient.AuthClient;

public interface IAuthApiClient
{
    Task<Result<RecoveryCodeInfo>> RegisterAsync(
        RegisterForm request,
        CancellationToken cancellationToken = default
    );

    Task<Result<AccessTokenInfo>> LoginAsync(
        LoginForm request,
        CancellationToken cancellationToken = default
    );
}
