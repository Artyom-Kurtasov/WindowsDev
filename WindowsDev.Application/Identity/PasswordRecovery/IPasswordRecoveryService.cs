using WindowsDev.Application.Primitives;

namespace WindowsDev.Application.Identity.PasswordRecovery;

public interface IPasswordRecoveryService
{
    Task<Result<bool>> IsRecoverCodeCorrectAsync(int recoveryCode, string login);
    Task<Result<(int, string)>> ChangePasswordAsync(string login, string password);
    Task<Result<bool>> IsUserExistAsync(string login);
}