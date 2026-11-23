using AutoMapper;
using System.Net.Http.Json;
using WindowsDev.Application.Primitives;
using WindowsDev.Contracts.Api.Common;
using WindowsDev.Contracts.Api.PasswordRecovery;
using WindowsDev.Contracts.UI.PasswordRecovery;

namespace WindowsDev.ApiClient.PasswordRecovery;

public class PasswordRecoveryApiClient : IPasswordRecoveryApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IMapper _mapper;

    public PasswordRecoveryApiClient(HttpClient httpClient, IMapper mapper)
    {
        _httpClient = httpClient;
        _mapper = mapper;
    }

    public async Task<Result<VerifyRecoveryCodeInfo>> IsRecoveryCodeCorrectAsync(
        VerifyRecoveryCodeForm request,
        CancellationToken cancellationToken
    )
    {
        var apiRequest = _mapper.Map<VerifyRecoveryCodeRequest>(request);

        using var response = await _httpClient.PostAsJsonAsync(
            "api/passwordrecovery/verify-code",
            apiRequest,
            cancellationToken
        );

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<VerifyRecoveryCodeResponse>(
                cancellationToken: cancellationToken
            );

            var info = _mapper.Map<VerifyRecoveryCodeInfo>(result);
            return Result<VerifyRecoveryCodeInfo>.Success(info);
        }

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        var errorCode = problem?.GetErrorCode() ?? string.Empty;
        return Result<VerifyRecoveryCodeInfo>.Failure(errorCode);
    }

    public async Task<Result<ResetPasswordInfo>> ChangePasswordAsync(
        ResetPasswordForm request,
        CancellationToken cancellationToken
    )
    {
        var apiRequest = _mapper.Map<ResetPasswordRequest>(request);

        using var response = await _httpClient.PostAsJsonAsync(
            "api/passwordrecovery/change-password",
            apiRequest,
            cancellationToken
        );

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ResetPasswordResponse>(
                cancellationToken: cancellationToken
            );

            var info = _mapper.Map<ResetPasswordInfo>(result);
            return Result<ResetPasswordInfo>.Success(info);
        }

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        var errorCode = problem?.GetErrorCode() ?? string.Empty;
        return Result<ResetPasswordInfo>.Failure(errorCode);
    }

    public async Task<Result<IsUserExistInfo>> IsUserExistAsync(
        IsUserExistForm request,
        CancellationToken cancellationToken
    )
    {
        var apiRequest = _mapper.Map<IsUserExistRequest>(request);

        using var response = await _httpClient.PostAsJsonAsync(
            "api/passwordrecovery/check-user",
            apiRequest,
            cancellationToken
        );

        var result = await response.Content.ReadFromJsonAsync<IsUserExistResponse>(
            cancellationToken: cancellationToken
        );

        var info = _mapper.Map<IsUserExistInfo>(result);
        return Result<IsUserExistInfo>.Success(info);
    }
}
