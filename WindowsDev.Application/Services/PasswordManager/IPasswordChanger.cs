using WindowsDev.Application.Primitives;

namespace WindowsDev.Application.Services.PasswordManager
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
