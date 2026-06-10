namespace WindowsDev.Business.Services.Profile.Interfaces
{
    public interface IProfileService
    {
        Task ChangePassword(string currentPassword, string newPassword, string confirmPassword);
        Task ChangeUsername(string currentUsername, string newUsername);
    }
}
