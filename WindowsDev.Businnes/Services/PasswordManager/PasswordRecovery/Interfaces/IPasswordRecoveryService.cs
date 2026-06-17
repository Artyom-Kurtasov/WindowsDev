namespace WindowsDev.Business.Services.PasswordManager.PasswordRecovery.Interfaces
{
    public interface IPasswordRecoveryService
    {
        int GenerateRecoveryCode();
        Task IsRecoverCodeCorrect(int recoveryCode, string login);
        Task<int> ChangePasswordAsync(string login, string password);
    }
}
