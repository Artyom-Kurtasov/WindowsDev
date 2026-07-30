using Microsoft.Extensions.Logging;
using WindowsDev.Logging;

namespace WindowsDev.Infrastructure.Logging
{
    public static partial class ProfileLogs
    {
        [LoggerMessage(
            EventId = EventIds.PasswordChangeFailed,
            Level = LogLevel.Error,
            Message = "Failed to change password for user {UserId}")]
        public static partial void PasswordChangeFailed(
            ILogger logger,
            int userId,
            Exception exception);

        [LoggerMessage(
            EventId = EventIds.UsernameChangeFailed,
            Level = LogLevel.Error,
            Message = "Failed to change username for user {UserId}")]
        public static partial void UsernameChangeFailed(
            ILogger logger,
            int userId,
            Exception exception);
    }
}