using WindowsDev.Logging.Api;

namespace WindowsDev.Api.Logging;

public static partial class PasswordRecoveryControllerLogs
{
    [LoggerMessage(
        EventId = PasswordRecoveryEventId.InvalidRecoveryCode,
        Level = LogLevel.Error,
        Message = "Recovery code for {login} is not valid"
    )]
    public static partial void InvalidRecoveryCode(ILogger logger, string login);

    [LoggerMessage(
        EventId = PasswordRecoveryEventId.RecoveryCodeCheckError,
        Level = LogLevel.Error,
        Message = "Unexpected error during password recovery code check for {login}"
    )]
    public static partial void UnexpectedError(ILogger logger, string login);

    [LoggerMessage(
        EventId = PasswordRecoveryEventId.ValidRecoveryCode,
        Level = LogLevel.Information,
        Message = "Recovery code for {login} is valid"
    )]
    public static partial void RecoveryCodeIsValid(ILogger logger, string login);

    [LoggerMessage(
        EventId = PasswordRecoveryEventId.PasswordRecoverySuccesful,
        Level = LogLevel.Information,
        Message = "Password recovery for {login} succesful"
    )]
    public static partial void PasswordRecoverySuccesful(ILogger logger, string login);
}