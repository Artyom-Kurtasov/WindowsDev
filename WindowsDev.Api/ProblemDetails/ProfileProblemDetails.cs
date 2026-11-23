namespace WindowsDev.Api.ProblemDetails;

internal static class ProfileProblemDetails
{
    public const string ChangePasswordConflict = "Change passwors conflict";

    public static string ChangePasswordConflictDetail(string login) =>
        $"The new password provided for user '{login}' is identical to the current one.";

    public const string ChangePasswordMismatch = "Change password mismatch";

    public static string ChangePasswordMismatchDetail(string login) =>
        $"The new password and confirmation password provided for user '{login}' do not match.";

    public const string UserNotFound = "User Not Found";

    public static string UserNotFoundDetail(string login) =>
        $"User with login '{login}' was not found.";

    public const string InvalidCurrentPassword = "Invalid Current Password";

    public static string InvalidCurrentPasswordDetail(string login) =>
        $"The current password provided for user '{login}' is invalid.";

    public const string ChangeUsernameConflict = "Change username conflict";

    public static string ChangeUsernameConflictDetail(string login) =>
        $"The new username provided for user '{login}' is already taken.";

    public static string ChangeUsernameSameAsCurrentDetail(string login) =>
        $"The new username provided for user '{login}' is identical to the current one.";

    public const string UnexpectedError = "Unexpected Error";

    public static string UnexpectedErrorDetail(string login) =>
        $"An unexpected error occurred during profile update for user '{login}'.";
}
