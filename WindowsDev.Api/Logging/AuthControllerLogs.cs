using WindowsDev.Logging.Api;

namespace WindowsDev.Api.Logging;

internal static partial class AuthControllerLogs
{
    [LoggerMessage(
        EventId = AuthEventId.LoginExist,
        Level = LogLevel.Warning,
        Message = "Login {Login} already exist"
    )]
    public static partial void LoginAlreadyExist(ILogger logger, string login);

    [LoggerMessage(
        EventId = AuthEventId.UsernameExist,
        Level = LogLevel.Warning,
        Message = "Username {Username} already exist"
    )]
    public static partial void UsernameAlreadyExist(ILogger logger, string username);

    [LoggerMessage(
        EventId = AuthEventId.RegistrationSucces,
        Level = LogLevel.Information,
        Message = "Registration was succesfull for {Login}"
    )]
    public static partial void RegistrationSucces(ILogger logger, string login);

    [LoggerMessage(
        EventId = AuthEventId.RegistrationFailed,
        Level = LogLevel.Warning,
        Message = "Registration failed for {Login}"
    )]
    public static partial void RegistrationFailed(ILogger logger, string login);
}
