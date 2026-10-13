using System.Net.Http.Json;
using WindowsDev.Application.Primitives;
using WindowsDev.Contracts.Api.Common;
using WindowsDev.Contracts.Api.PasswordRecovery;

namespace WindowsDev.ApiClient.PasswordRecovery;

public record IsUserExistRequest(string login);
public record IsUserExistResponse(bool isExist);

public class PasswordRecoveryApiClient : IPasswordRecoveryApiClient
{
    private readonly HttpClient _httpClient;

    public PasswordRecoveryApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result<VerifyRecoveryCodeResponse>> IsRecoveryCodeCorrectAsync(
        VerifyRecoveryCodeRequest request,
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
            var result = await response.Content.ReadFromJsonAsync<VerifyRecoveryCodeResponse>(
                cancellationToken: cancellationToken
            );
            return Result<VerifyRecoveryCodeResponse>.Success(result);
        }

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        var errorCode = problem?.GetErrorCode() ?? string.Empty;
        return Result<VerifyRecoveryCodeResponse>.Failure(errorCode);
    }

    public async Task<Result<ResetPasswordResponse>> ChangePasswordAsync(
        ResetPasswordRequest request,
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
            var result = await response.Content.ReadFromJsonAsync<ResetPasswordResponse>(
                cancellationToken: cancellationToken
            );
            return Result<ResetPasswordResponse>.Success(result);
        }

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        var errorCode = problem?.GetErrorCode() ?? string.Empty;
        return Result<ResetPasswordResponse>.Failure(errorCode);
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
