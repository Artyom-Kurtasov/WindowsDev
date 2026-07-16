using WindowsDev.Business.Primitives;

namespace WindowsDev.Business.Services.PasswordManager.PasswordRecovery.Interfaces
{
    public interface IPasswordRecoveryService
    {
        Task<Result<bool>> IsRecoverCodeCorrectAsync(int recoveryCode, string login);
        Task<Result<int>> ChangePasswordAsync(string login, string password);
        Task<bool> IsUserExistAsync(string login);
    }
}
