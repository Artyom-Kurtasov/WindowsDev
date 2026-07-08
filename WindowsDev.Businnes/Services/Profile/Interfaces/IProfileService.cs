using WindowsDev.Business.Primitives;

namespace WindowsDev.Business.Services.Profile.Interfaces
{
    public interface IProfileService
    {
        Task<Result<bool>> ChangePasswordAsync(string currentPassword, string newPassword, string confirmPassword);
        Task<Result<bool>> ChangeUsernameAsync(string currentUsername, string newUsername);
    }
}
