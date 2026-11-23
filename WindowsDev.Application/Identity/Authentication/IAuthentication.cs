using WindowsDev.Application.Primitives;
using WindowsDev.Domain.Entities;

namespace WindowsDev.Application.Identity.Authentication;

public interface IAuthentication
{
    Task<Result<(string, UserRefreshToken)>> Authenticate(string login, string password);
    Task<Result<(string, UserRefreshToken)>> RefreshAsync(UserRefreshToken refreshToken);
}