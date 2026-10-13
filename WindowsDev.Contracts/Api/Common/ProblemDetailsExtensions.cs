namespace WindowsDev.Contracts.Api.Common;

public static class ProblemDetailsExtensions
{

    public static string? GetErrorCode(this ProblemDetails problemDetails) =>
        problemDetails?.ErrorMessageCode ?? string.Empty;
}
