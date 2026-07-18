using Microsoft.Extensions.Logging;

namespace WindowsDev.Infrastructure.Logging
{
    public static partial class AppXamlLogs
    {
        [LoggerMessage(
            EventId = EventIds.DispatcherUnhandledException,
            Level = LogLevel.Error,
            Message = "Unhandled UI dispatcher exception occurred"
        )]
        internal static partial void LogDispatcherUnhandledException(
            ILogger logger,
            Exception exception
        );

        [LoggerMessage(
            EventId = EventIds.AppDomainUnhandledException,
            Level = LogLevel.Error,
            Message = "Unhandled AppDomain exception occurred"
        )]
        internal static partial void LogAppDomainUnhandledException(
            ILogger logger,
            Exception exception
        );

        [LoggerMessage(
            EventId = EventIds.UnobservedTaskException,
            Level = LogLevel.Error,
            Message = "Unobserved task exception occurred"
        )]
        internal static partial void LogUnobservedTaskException(
            ILogger logger,
            Exception exception
        );

        [LoggerMessage(
            EventId = EventIds.DatabaseWarmUpFailed,
            Level = LogLevel.Error,
            Message = "Database warm-up failed"
        )]
        internal static partial void LogDatabaseWarmUpFailed(ILogger logger, Exception exception);
    }
}
