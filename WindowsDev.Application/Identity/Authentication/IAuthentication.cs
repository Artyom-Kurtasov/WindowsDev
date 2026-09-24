using WindowsDev.Application.Primitives;
using WindowsDev.Domain.Entities;

namespace WindowsDev.Application.Identity.Authentication;

public interface IAuthentication
{
    Task<Result<string>> Authenticate(string login, string password);
}