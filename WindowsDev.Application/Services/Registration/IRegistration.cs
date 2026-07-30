using WindowsDev.Application.Primitives;

namespace WindowsDev.Application.Services.Registration
{
    public interface IRegistration
    {
        Task<Result<int>> Register(string password, string login, string username);
        Task<bool> IsLoginAvailableAsync(string login);
        Task<bool> IsUsernameAvailableAsync(string username);
    }
}
