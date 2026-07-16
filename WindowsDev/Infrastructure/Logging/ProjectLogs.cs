using Microsoft.Extensions.Logging;

namespace WindowsDev.Infrastructure.Logging
{
    public static partial class ProjectLogs
    {
        [LoggerMessage(
            EventId = EventIds.ProjectCreationFailed,
            Level = LogLevel.Error,
            Message = "Failed to create project '{ProjectName}'")]
        internal static partial void ProjectCreationFailed(
            ILogger logger,
            string projectName,
            Exception exception);

        [LoggerMessage(
            EventId = EventIds.ProjectDeleteFailed,
            Level = LogLevel.Error,
            Message = "Failed to delete project {ProjectId}")]
        internal static partial void ProjectDeleteFailed(
            ILogger logger,
            int projectId,
            Exception exception);

        [LoggerMessage(
            EventId = EventIds.ProjectLoadFailed,
            Level = LogLevel.Error,
            Message = "Failed to load projects")]
        internal static partial void ProjectLoadFailed(
            ILogger logger,
            Exception exception);
    }
}