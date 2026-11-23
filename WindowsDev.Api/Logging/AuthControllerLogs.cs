using WindowsDev.Api.Logging.EventId;

namespace WindowsDev.Api.Logging;

internal static partial class AuthControllerLogs
{
    [LoggerMessage(
        EventId = AuthControllerEventId.LoginExist,
        Level = LogLevel.Warning,
        Message = "LoginAsync {Login} already exist"
    )]
    public static partial void LoginAlreadyExist(ILogger logger, string login);

    [LoggerMessage(
        EventId = AuthControllerEventId.UsernameExist,
        Level = LogLevel.Warning,
        Message = "Username {Username} already exist"
    )]
    public static partial void UsernameAlreadyExist(ILogger logger, string username);

    [LoggerMessage(
        EventId = AuthControllerEventId.RegistrationSucces,
        Level = LogLevel.Information,
        Message = "Registration was succesfull for {Login}"
    )]
    public static partial void RegistrationSuccess(ILogger logger, string login);

    [LoggerMessage(
        EventId = AuthControllerEventId.RegistrationFailed,
        Level = LogLevel.Warning,
        Message = "Error during registration for {Login}"
    )]
    public static partial void RegistrationError(ILogger logger, string login);

    [LoggerMessage(
        EventId = AuthControllerEventId.LoginFailed,
        Level = LogLevel.Warning,
        Message = "Failed login attempt for {Login}"
    )]
    public static partial void LoginFailed(ILogger logger, string login);

    [LoggerMessage(
        EventId = AuthControllerEventId.LoginSucces,
        Level = LogLevel.Warning,
        Message = "Succes login attempt for {Login}"
    )]
    public static partial void LoginSuccess(ILogger logger, string login);

    [LoggerMessage(
    EventId = AuthControllerEventId.RefreshFailed,
    Level = LogLevel.Warning,
    Message = "Failed refresh token attempt"
)]
    public static partial void RefreshFailed(ILogger logger);

    [LoggerMessage(
        EventId = AuthControllerEventId.RefreshSucces,
        Level = LogLevel.Information,
        Message = "Successful refresh token for user {UserId}"
    )]
    public static partial void RefreshSuccess(ILogger logger, int userId);
}
