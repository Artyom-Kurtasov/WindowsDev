using WindowsDev.Logging.Api;

namespace WindowsDev.Api.Logging;

internal partial class AttachmentsControllerLogs
{
    [LoggerMessage(
        EventId = AuthEventId.LoginExist,
        Level = LogLevel.Warning,
        Message = "!!" 
    )]
    public static partial void LoginAlreadyExist(ILogger logger, string login);
}
