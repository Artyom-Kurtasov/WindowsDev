using WindowsDev.Application.Primitives;

namespace WindowsDev.Application.Services.Profile
{
    public interface IProfileService
    {
        Task<Result<int>> ChangePasswordAsync(string currentPassword, string newPassword, string confirmPassword);
        Task<Result<bool>> ChangeUsernameAsync(string currentUsername, string newUsername);
    }
}
