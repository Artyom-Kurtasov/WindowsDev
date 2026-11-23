using WindowsDev.Api.Logging.EventId;

namespace WindowsDev.Api.Logging;

public static partial class CommonLogs
{
    [LoggerMessage(
        EventId = CommonEventId.UserNotFound,
        Level = LogLevel.Error,
        Message = "User with login {login} not found"
    )]
    public static partial void UserNotFound(ILogger logger, string login);

    [LoggerMessage(
        EventId = CommonEventId.InternalServerError,
        Level = LogLevel.Error,
        Message = "An unexpected error occurred: {message}"
    )]
    public static partial void InternalServerError(ILogger logger, string message);
}
