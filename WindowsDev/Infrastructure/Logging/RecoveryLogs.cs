using Microsoft.Extensions.Logging;

namespace WindowsDev.Infrastructure.Logging
{
    public static partial class RecoveryLogs
    {
        [LoggerMessage(
            EventId = EventIds.PasswordRecoveryFailed,
            Level = LogLevel.Error,
            Message = "Password recovery failed for user '{Login}'")]
        internal static partial void PasswordRecoveryFailed(
            ILogger logger,
            string login,
            Exception exception);

        [LoggerMessage(
            EventId = EventIds.InvalidRecoveryCode,
            Level = LogLevel.Warning,
            Message = "Invalid recovery code provided for user '{Login}'")]
        internal static partial void InvalidRecoveryCode(
            ILogger logger,
            string login);

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