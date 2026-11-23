using WindowsDev.Application.Primitives;
using WindowsDev.Domain.Entities;

namespace WindowsDev.Application.Identity.PasswordChanger;

public interface IPasswordChanger
{
    bool IsRecoveryMode { get; set; }
    Task<Result<(int, string, UserRefreshToken)>> ChangeUserPasswordAsync(
        string login,
        string newPassword,
        string currentPassword = ""
    );
    int GenerateRecoveryCode();
}