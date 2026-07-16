using WindowsDev.Business.Primitives;

namespace WindowsDev.Business.Services.Registration.Interfaces
{
    public interface IRegistration
    {
        Task<Result<int>> Register(string password, string login, string username);
        Task<bool> IsLoginAvailableAsync(string login);
        Task<bool> IsUsernameAvailableAsync(string username);
    }
}
