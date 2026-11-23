namespace WindowsDev.Api.ProblemDetails;

internal static class TaskProblemDetails
{
    public const string TaskNotFound = "Task not found.";
    public static string TaskNotFoundDetail(int id) => $"Task with ID {id} was not found. Could it have been deleted?";
}
