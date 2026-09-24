using WindowsDev.Api.Logging.EventId;
using WindowsDev.Logging.Api;

namespace WindowsDev.Api.Logging;

internal static partial class ProjectsControllerLogs
{
    [LoggerMessage(
        EventId = ProjectsControllerEventId.AddSuccessful,
        Level = LogLevel.Information,
        Message = "Project was add successful. user login: {login}, project id: {projectId}"
    )]
    public static partial void AddSuccessful(ILogger logger, string login, int projectId);

    [LoggerMessage(
        EventId = ProjectsControllerEventId.DeleteSuccessful,
        Level = LogLevel.Information,
        Message = "Project was delete successful."
    )]
    public static partial void DeleteSuccessful(ILogger logger);

    [LoggerMessage(
        EventId = ProjectsControllerEventId.AddUnauthorized,
        Level = LogLevel.Error,
        Message = "Unauthorized request. Unable to recover user data. UserId: {rawUserId}, userLogin {userLogin}"
    )]
    public static partial void AddUnauthorized(
        ILogger logger,
        string? rawUserId,
        string? userLogin
    );

    [LoggerMessage(
        EventId = ProjectsControllerEventId.UpdateSuccessful,
        Level = LogLevel.Information,
        Message = "Project was update successful. ProjectId: {projectId}"
    )]
    public static partial void UpdateSuccessful(ILogger logger, int projectId);

    [LoggerMessage(
        EventId = ProjectsControllerEventId.ProjectNotFound,
        Level = LogLevel.Warning,
        Message = "Project with id {projectId} not found"
    )]
    public static partial void ProjectNotFound(ILogger logger, int projectId);

    [LoggerMessage(
        EventId = ProjectsControllerEventId.GetProjectsUnauthorized,
        Level = LogLevel.Error,
        Message = "Unauthorized request. Unable to recover user data. UserId: {rawUserId}"
    )]
    public static partial void GetProjectsUnauthorized(ILogger logger, string? rawUserId);

    [LoggerMessage(
        EventId = ProjectsControllerEventId.GetProjectsSuccessful,
        Level = LogLevel.Information,
        Message = "The projects were received successfully. Page: {page}, page size: {pageSize}, search filter: {searchFilter}, user id: {userId}"
    )]
    public static partial void GetProjectsSuccessful(ILogger logger, int page, int pageSize, string? searchFilter, int userId);

    [LoggerMessage(
        EventId = ProjectsControllerEventId.GetProjectsCountUnauthorized,
        Level = LogLevel.Error,
        Message = "Unauthorized request. Unable to recover user data. UserId: {rawUserId}"
    )]
    public static partial void GetProjectsCountUnauthorized(ILogger logger, string? rawUserId);

    [LoggerMessage(
        EventId = ProjectsControllerEventId.GetProjectsCountSuccessful,
        Level = LogLevel.Information,
        Message = "The projects count were received successfuly. User id: {userId}"
    )]
    public static partial void GetProjectsCountSuccessful(ILogger logger, int userId);

    [LoggerMessage(
        EventId = ProjectsControllerEventId.GetProjectSuccessful,
        Level = LogLevel.Information,
        Message = "The project was received successfully. Project id: {projectId}"
    )]
    public static partial void GetProjectSuccessful(ILogger logger, int projectId);
}
