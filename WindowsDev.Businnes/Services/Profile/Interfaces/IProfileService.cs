namespace WindowsDev.Business.Services.Profile.Interfaces
{
    public interface IProfileService
    {
        Task ChangePasswordAsync(string currentPassword, string newPassword, string confirmPassword);
        Task ChangeUsernameAsync(string currentUsername, string newUsername);
    }
}
