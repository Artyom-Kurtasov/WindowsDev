using Microsoft.Extensions.Logging;
using WindowsDev.Logging;

namespace WindowsDev.Infrastructure.Logging
{
    public static partial class ProjectLogs
    {
        [LoggerMessage(
            EventId = EventIds.ProjectCreationFailed,
            Level = LogLevel.Error,
            Message = "Failed to create project '{ProjectName}'")]
        public static partial void ProjectCreationFailed(
            ILogger logger,
            string projectName,
            Exception exception);

        [LoggerMessage(
            EventId = EventIds.ProjectDeleteFailed,
            Level = LogLevel.Error,
            Message = "Failed to delete project {ProjectId}")]
        public static partial void ProjectDeleteFailed(
            ILogger logger,
            int projectId,
            Exception exception);

        [LoggerMessage(
            EventId = EventIds.ProjectLoadFailed,
            Level = LogLevel.Error,
            Message = "Failed to load projects")]
        public static partial void ProjectLoadFailed(
            ILogger logger,
            Exception exception);
    }
}