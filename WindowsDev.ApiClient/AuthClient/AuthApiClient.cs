using AutoMapper;
using System.Net.Http.Json;
using WindowsDev.Application.Primitives;
using WindowsDev.Contracts.Api.Common;
using WindowsDev.Contracts.Api.Identity;
using WindowsDev.Contracts.UI;

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

    public async Task<Result<AccessTokenInfo>> LoginAsync(
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
                cancellationToken: cancellationToken);
            return Result<AccessTokenInfo>.Success(new(result.JwtToken));
        }

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        var errorCode = problem?.GetErrorCode() ?? string.Empty;
        return Result<AccessTokenInfo>.Failure(errorCode);
    }

    public async Task<Result<RecoveryCodeInfo>> RegisterAsync(
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
                cancellationToken: cancellationToken);

            return Result<RecoveryCodeInfo>.Success(new(result.RecoveryCode));
        }

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        var errorCode = problem?.GetErrorCode() ?? string.Empty;
        return Result<RecoveryCodeInfo>.Failure(errorCode);
    }
}
