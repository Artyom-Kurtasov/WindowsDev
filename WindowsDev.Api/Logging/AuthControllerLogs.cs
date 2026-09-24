using WindowsDev.Logging.Api;

namespace WindowsDev.Api.Logging;

internal static partial class AuthControllerLogs
{
    [LoggerMessage(
        EventId = AuthEventId.LoginExist,
        Level = LogLevel.Warning,
        Message = "LoginAsync {Login} already exist"
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
    public static partial void RegistrationSuccess(ILogger logger, string login);

    [LoggerMessage(
        EventId = AuthEventId.RegistrationFailed,
        Level = LogLevel.Warning,
        Message = "Error during registration for {Login}"
    )]
    public static partial void RegistrationError(ILogger logger, string login);

    [LoggerMessage(
        EventId = AuthEventId.LoginFailed,
        Level = LogLevel.Warning,
        Message = "Failed login attempt for {Login}"
    )]
    public static partial void LoginFailed(ILogger logger, string login);

    [LoggerMessage(
        EventId = AuthEventId.LoginSucces,
        Level = LogLevel.Warning,
        Message = "Succes login attempt for {Login}"
    )]
    public static partial void LoginSuccess(ILogger logger, string login);

    [LoggerMessage(
        EventId = AuthEventId.LoginSucces,
        Level = LogLevel.Warning,
        Message = "Error during loginfor {Login}"
    )]
    public static partial void LoginError(ILogger logger, string login);
}
