using WindowsDev.Api.Logging.EventId;

namespace WindowsDev.Api.Logging;

internal static partial class ProfilesControllerLogs
{
    [LoggerMessage(
        EventId = ProfilesControllerEventId.ChangePasswordSuccessful,
        Level = LogLevel.Information,
        Message = "Password was changed successful. User login: {login}"
    )]
    public static partial void ChangePasswordSuccessful(ILogger logger, string login);

    [LoggerMessage(
        EventId = ProfilesControllerEventId.ChangePasswordFailed,
        Level = LogLevel.Warning,
        Message = "Password change failed. User login: {login}"
    )]
    public static partial void ChangePasswordFailed(ILogger logger, string login);

    [LoggerMessage(
        EventId = ProfilesControllerEventId.ChangeUsernameSuccessful,
        Level = LogLevel.Information,
        Message = "Username was changed successful. User login: {login}"
    )]
    public static partial void ChangeUsernameSuccessful(ILogger logger, string login);

    [LoggerMessage(
        EventId = ProfilesControllerEventId.ChangeUsernameFailed,
        Level = LogLevel.Warning,
        Message = "Username change failed. User login: {login}"
    )]
    public static partial void ChangeUsernameFailed(ILogger logger, string login);

    [LoggerMessage(
        EventId = ProfilesControllerEventId.GetProfileSuccessful,
        Level = LogLevel.Information,
        Message = "The profile was received successfully. User id: {userId}"
    )]
    public static partial void GetProfileSuccessful(ILogger logger, int userId);
}
