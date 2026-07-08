using Microsoft.Extensions.Logging;

namespace WindowsDev.Infrastructure.Logging
{
    public static partial class AuthLogs
    {
        [LoggerMessage(
            EventId = EventIds.AuthorizationFailed,
            Level = LogLevel.Error,
            Message = "An unexpected error occurred during authorization.")]
        public static partial void AuthorizationFailed(ILogger logger, Exception exception);

        [LoggerMessage(
            EventId = EventIds.RegistrationFailed,
            Level = LogLevel.Error,
            Message = "An unexpected error occurred during registration.")]
        public static partial void RegistrationFailed(ILogger logger, Exception exception);

        [LoggerMessage(
            EventId = EventIds.LoginAvailabilityCheckFailed,
            Level = LogLevel.Error,
            Message = "Login availability check failed.")]
        public static partial void LoginAvailabilityCheckFailed(ILogger logger, Exception exception);

        [LoggerMessage(
            EventId = EventIds.UsernameAvailabilityCheckFailed,
            Level = LogLevel.Error,
            Message = "Username availability check failed.")]
        public static partial void UsernameAvailabilityCheckFailed(ILogger logger, Exception exception);
    }
}
