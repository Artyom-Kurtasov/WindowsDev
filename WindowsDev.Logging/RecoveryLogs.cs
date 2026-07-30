using Microsoft.Extensions.Logging;
using WindowsDev.Logging;

namespace WindowsDev.Infrastructure.Logging
{
    public static partial class RecoveryLogs
    {
        [LoggerMessage(
            EventId = EventIds.PasswordResetFailed,
            Level = LogLevel.Error,
            Message = "Failed to reset password for user '{Login}'"
        )]
        public static partial void PasswordResetFailed(
            ILogger logger,
            string login,
            Exception exception
        );
    }
}
