using WindowsDev.Application.Primitives;

namespace WindowsDev.Application.Users;

public interface IProfileService
{
    Task<Result<(int, string)>> ChangePasswordAsync(string currentPassword, string newPassword, string confirmPassword);
    Task<Result<string>> ChangeUsernameAsync(string currentUsername, string newUsername);
}