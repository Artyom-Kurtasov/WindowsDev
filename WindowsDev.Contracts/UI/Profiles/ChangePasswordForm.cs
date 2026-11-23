namespace WindowsDev.Contracts.UI.Profiles;

public record ChangePasswordForm(
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword,
    string Login);
