using WindowsDev.Application.Primitives;

namespace WindowsDev.Application.Services.PasswordManager.PasswordRecovery
{
    public interface IPasswordRecoveryService
    {
        Task<Result<bool>> IsRecoverCodeCorrectAsync(int recoveryCode, string login);
        Task<Result<int>> ChangePasswordAsync(string login, string password);
        Task<bool> IsUserExistAsync(string login);
    }
}
