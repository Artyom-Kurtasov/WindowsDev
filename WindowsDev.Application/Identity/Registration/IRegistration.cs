using WindowsDev.Application.Primitives;
using WindowsDev.Domain.Entities;

namespace WindowsDev.Application.Identity.Registration;

public interface IRegistration
{
    Task<Result<(int, string, UserRefreshToken)>> Register(string password, string login, string username);
    Task<bool> IsLoginAvailableAsync(string login);
    Task<bool> IsUsernameAvailableAsync(string username);
}