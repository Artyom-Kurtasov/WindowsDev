using Microsoft.Extensions.Logging;

namespace WindowsDev.Infrastructure.Logging
{
    public static partial class ProfileLogs
    {
        [LoggerMessage(
            EventId = EventIds.ProfileUpdateFailed,
            Level = LogLevel.Error,
            Message = "Failed to update profile for user {UserId}")]
        internal static partial void ProfileUpdateFailed(
            ILogger logger,
            int userId,
            Exception exception);

        [LoggerMessage(
            EventId = EventIds.PasswordChangeFailed,
            Level = LogLevel.Error,
            Message = "Failed to change password for user {UserId}")]
        internal static partial void PasswordChangeFailed(
            ILogger logger,
            int userId,
            Exception exception);

        [LoggerMessage(
            EventId = EventIds.UsernameChangeFailed,
            Level = LogLevel.Error,
            Message = "Failed to change username for user {UserId}")]
        internal static partial void UsernameChangeFailed(
            ILogger logger,
            int userId,
            Exception exception);

        [LoggerMessage(
            EventId = EventIds.ProfileLoadFailed,
            Level = LogLevel.Error,
            Message = "Failed to load profile for user {UserId}")]
        internal static partial void ProfileLoadFailed(
            ILogger logger,
            int userId,
            Exception exception);
    }
}