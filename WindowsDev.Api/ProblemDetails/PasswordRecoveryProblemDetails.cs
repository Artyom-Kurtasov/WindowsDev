namespace WindowsDev.Api.ProblemDetails;

internal static class PasswordRecoveryProblemDetails
{
    public const string InvalidRecoveryCode = "Invalid Recovery Code";
    public static string InvalidRecoveryCodeDetail(string login) => $"The recovery code provided for user '{login}' is invalid or has expired.";
    public const string UnexpectedError = "Unexpected Error";
    public static string UnexpectedErrorDetail(string login) => $"An unexpected error occurred during code verification for user '{login}'.";
    public const string UserNotFound = "User Not Found";
    public static string UserNotFoundDetail(string login) => $"User with login '{login}' was not found.";
    public const string PasswordChangeFailed = "Password Change Failed";
    public static string PasswordChangeFailedDetail(string login) => $"An unexpected error occurred during password change for user '{login}'.";
}
