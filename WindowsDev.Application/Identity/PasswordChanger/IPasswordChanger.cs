using WindowsDev.Application.Primitives;

namespace WindowsDev.Application.Identity.PasswordChanger;

public interface IPasswordChanger
{
    bool IsRecoveryMode { get; set; }
    Task<Result<(int, string)>> ChangeUserPasswordAsync(
        string login,
        string newPassword,
        string currentPassword = ""
    );
    int GenerateRecoveryCode();
}