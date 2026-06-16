namespace WindowsDev.Business.Services.PasswordManager.PasswordRecovery
{
    public interface IPasswordRecoveryService
    {
        int GenerateRecoveryCode();
        Task<bool> IsRecoverCodeCorrect(int recoveryCode, string login);
        Task<int> ChangePasswordAsync(string login, string password);
    }
}
