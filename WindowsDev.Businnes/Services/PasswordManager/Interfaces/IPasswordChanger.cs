using WindowsDev.Business.Primitives;

namespace WindowsDev.Business.Services.PasswordManager.Interfaces
{
    public interface IPasswordChanger
    {
        bool IsRecoveryMode { get; set; }
        Task<Result<int>> ChangeUserPasswordAsync(
            string login,
            string newPassword,
            string currentPassword = ""
        );
        int GenerateRecoveryCode();
    }
}
