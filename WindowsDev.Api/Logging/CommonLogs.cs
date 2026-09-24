using WindowsDev.Logging.Api;

namespace WindowsDev.Api.Logging;

public static partial class CommonLogs
{
    [LoggerMessage(
        EventId = CommonEventId.UserNotFound,
        Level = LogLevel.Error,
        Message = "User with login {login} not found"
    )]
    public static partial void UserNotFound(ILogger logger, string login);
}
