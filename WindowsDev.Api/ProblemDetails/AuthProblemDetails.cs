namespace WindowsDev.Api.ProblemDetails;

public static class AuthProblemDetails
{
    public const string LoginConflict = "Login Conflict";
    public static string LoginConflictDetail(string login) => $"This login {login} is already taken.";
    public const string UsernameConflict = "Username Conflict";
    public static string UsernameConflictDetail(string username) => $"This username {username} is already taken.";
    public const string InvalidCredentials = "Invalid Credentials";
    public const string InvalidCredentialsDetail = "Invalid username or password. Please try again.";
}
