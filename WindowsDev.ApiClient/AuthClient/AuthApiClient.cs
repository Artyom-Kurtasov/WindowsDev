using AutoMapper;
using System.Net.Http.Json;
using WindowsDev.Application.Primitives;
using WindowsDev.Contracts.Api.Common;
using WindowsDev.Contracts.Api.Identity;
using WindowsDev.Contracts.UI.Identity;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;

namespace WindowsDev.ApiClient.AuthClient;

public class AuthApiClient : IAuthApiClient
{
    private readonly IMapper _mapper;
    private readonly HttpClient _httpClient;

    public AuthApiClient(HttpClient httpClient, IMapper mapper)
    {
        _httpClient = httpClient;
        _mapper = mapper;
    }

    public async Task<Result<TokensInfo>> RefreshAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default
    )
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "api/auth/refresh",
            request,
            cancellationToken
        );

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<UserLoginResponse>(
                cancellationToken: cancellationToken
            );

            var tokens = _mapper.Map<TokensInfo>(result);
            return Result<TokensInfo>.Success(tokens);
        }

        if (response.Content.Headers.ContentLength > 0)
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(
                cancellationToken
            );
            var errorCode = problem?.GetErrorCode() ?? string.Empty;
            return Result<TokensInfo>.Failure(errorCode);
        }

        return Result<TokensInfo>.Failure(CommonErrors.InvalidSession);
    }

    public async Task<Result<TokensInfo>> LoginAsync(
        LoginForm request,
        CancellationToken cancellationToken = default
    )
    {
        var loginRequest = _mapper.Map<UserLoginRequest>(request);
        using var response = await _httpClient.PostAsJsonAsync(
            "api/auth/login",
            loginRequest,
            cancellationToken
        );

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<UserLoginResponse>(
                cancellationToken: cancellationToken
            );

            var tokens = _mapper.Map<TokensInfo>(result);
            return Result<TokensInfo>.Success(tokens);
        }

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        var errorCode = problem?.GetErrorCode() ?? string.Empty;
        return Result<TokensInfo>.Failure(errorCode);
    }

    public async Task<Result<RegisterInfo>> RegisterAsync(
        RegisterForm request,
        CancellationToken cancellationToken = default
    )
    {
        var registerRequest = _mapper.Map<UserRegisterRequest>(request);
        using var response = await _httpClient.PostAsJsonAsync(
            "api/auth/register",
            request,
            cancellationToken
        );

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<UserRegisterResponse>(
                cancellationToken: cancellationToken
            );

            var info = _mapper.Map<RegisterInfo>(result);
            return Result<RegisterInfo>.Success(new RegisterInfo(info.RecoveryCode, info.JwtToken, info.RefreshToken));
        }

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        var errorCode = problem?.GetErrorCode() ?? string.Empty;
        return Result<RegisterInfo>.Failure(errorCode);
    }

    public async Task<Result<AvailabilityInfo>> IsUsernameAvailableAsync(
        UsernameAvailabilityForm form,
        CancellationToken cancellationToken = default
    )
    {
        var request = _mapper.Map<IsUsernameAvailableRequest>(form);
        using var response = await _httpClient.PostAsJsonAsync(
            "api/auth/isUsernameAvailable",
            request,
            cancellationToken
        );
        var result = await response.Content.ReadFromJsonAsync<IsUsernameAvailableResponse>(
            cancellationToken: cancellationToken
        );

        var info = _mapper.Map<AvailabilityInfo>(result);
        return Result<AvailabilityInfo>.Success(info);
    }

    public async Task<Result<AvailabilityInfo>> IsLoginAvailableAsync(
        LoginAvailabilityForm form,
        CancellationToken cancellationToken = default
    )
    {
        var request = _mapper.Map<IsLoginAvailableRequest>(form);
        using var response = await _httpClient.PostAsJsonAsync(
            "api/auth/isLoginAvailable",
            request,
            cancellationToken
        );

        var result = await response.Content.ReadFromJsonAsync<IsLoginAvailableResponse>(
            cancellationToken: cancellationToken
        );

        var info = _mapper.Map<AvailabilityInfo>(result);
        return Result<AvailabilityInfo>.Success(info);
    }
}
