using Microsoft.AspNetCore.Mvc;

namespace WindowsDev.Api.Common;

public static class ProblemDetailsExtensions
{
    private const string _errorCode = "errorCode";

    public static string? GetErrorCode(this ProblemDetails problemDetails) =>
        problemDetails.Extensions.TryGetValue(_errorCode, out var errorCode)
            ? errorCode?.ToString()
            : null;
}
