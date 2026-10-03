using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using WindowsDev.Api.Common;
using WindowsDev.Api.DTO.Request;
using WindowsDev.Api.DTO.Response;
using WindowsDev.Application.Primitives;

namespace WindowsDev.ApiClients.PasswordRecoveryClient;

internal class PasswordRecoveryApiClient : IPasswordRecoveryApiClient
{
    private readonly HttpClient _httpClient;

    public PasswordRecoveryApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result<VerifyPasswordRecoveryCodeResponse>> IsRecoveryCodeCorrectAsync(
        VerifyPasswordRecoveryCodeRequest request,
        CancellationToken cancellationToken
    )
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "api/passwordrecovery/verify-code",
            request,
            cancellationToken
        );

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<VerifyPasswordRecoveryCodeResponse>(
                cancellationToken: cancellationToken
            );
            return Result<VerifyPasswordRecoveryCodeResponse>.Success(result);
        }

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        var errorCode = problem?.GetErrorCode() ?? string.Empty;
        return Result<VerifyPasswordRecoveryCodeResponse>.Failure(errorCode);
    }

    public async Task<Result<ChangePasswordResponse>> ChangePasswordAsync(
        ChangePasswordRequest request,
        CancellationToken cancellationToken
    )
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "api/passwordrecovery/change-password",
            request,
            cancellationToken
        );

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ChangePasswordResponse>(
                cancellationToken: cancellationToken
            );
            return Result<ChangePasswordResponse>.Success(result);
        }

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        var errorCode = problem?.GetErrorCode() ?? string.Empty;
        return Result<ChangePasswordResponse>.Failure(errorCode);
    }

    public async Task<IsUserExistResponse> IsUserExistAsync(
        IsUserExistRequest request,
        CancellationToken cancellationToken
    )
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "api/passwordrecovery/check-user",
            request,
            cancellationToken
        );

        var result = await response.Content.ReadFromJsonAsync<IsUserExistResponse>(
            cancellationToken: cancellationToken
        );
        return result!;
    }
}
