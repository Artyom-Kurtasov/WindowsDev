using WindowsDev.Business.Primitives;

namespace WindowsDev.Business.Services.PasswordManager.PasswordRecovery.Interfaces
{
    public interface IPasswordRecoveryService
    {
        int GenerateRecoveryCode();
        Task<Result<bool>> IsRecoverCodeCorrect(int recoveryCode, string login);
        Task<Result<int>> ChangePasswordAsync(string login, string password);
    }
}
