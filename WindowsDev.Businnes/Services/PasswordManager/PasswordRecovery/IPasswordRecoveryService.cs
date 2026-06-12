namespace WindowsDev.Business.Services.PasswordManager.PasswordRecovery
{
    public interface IPasswordRecoveryService
    {
        int GenerateRecoveryCode();
        Task<bool> IsRecoverCodeCorrect(int recoveryCode, string login);
        Task ChangePasswordAsync(string login, string password);
    }
}
