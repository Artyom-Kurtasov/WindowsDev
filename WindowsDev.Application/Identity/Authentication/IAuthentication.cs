using WindowsDev.Application.Primitives;

namespace WindowsDev.Application.Identity.Authentication;

public interface IAuthentication
{
    Task<Result<string>> Authenticate(string login, string password);
}