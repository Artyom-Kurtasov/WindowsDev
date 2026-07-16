using Microsoft.Extensions.Logging;

namespace WindowsDev.Infrastructure.Logging
{
    public static partial class RecoveryLogs
    {
        [LoggerMessage(
            EventId = EventIds.PasswordResetFailed,
            Level = LogLevel.Error,
            Message = "Failed to reset password for user '{Login}'")]
        internal static partial void PasswordResetFailed(
            ILogger logger,
            string login,
            Exception exception);
    }
}