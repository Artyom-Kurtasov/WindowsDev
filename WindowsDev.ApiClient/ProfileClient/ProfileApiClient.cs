using System.Net.Http.Json;
using WindowsDev.Application.Identity.Cryptography;
using WindowsDev.Application.Primitives;
using WindowsDev.Contracts.Api.Common;
using WindowsDev.Contracts.Api.Profiles;

namespace WindowsDev.ApiClient.ProfileClient;

public class ProfileApiClient : IProfileApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ISecureTokenStorage _tokenStorage;

    public ProfileApiClient(HttpClient httpClient, ISecureTokenStorage tokenStorage)
    {
        _httpClient = httpClient;
        _tokenStorage = tokenStorage;
    }

    public async Task<Result<ChangePasswordResponse>> ChangePasswordAsync(
        ChangePasswordRequest request,
        CancellationToken cancellationToken
    )
    {
        AppendAuthorizationHeader();
        using var response = await _httpClient.PostAsJsonAsync(
            "api/profiles/changePassword",
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

    public async Task<Result<ChangeUsernameResponse>> ChangeUsernameAsync(
        ChangeUsernameRequest request,
        CancellationToken cancellationToken
    )
    {
        AppendAuthorizationHeader();
        using var response = await _httpClient.PostAsJsonAsync(
            "api/profiles/changeUsername",
            request,
            cancellationToken
        );

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ChangeUsernameResponse>(
                cancellationToken: cancellationToken
            );

            return Result<ChangeUsernameResponse>.Success(result);
        }

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        var errorCode = problem?.GetErrorCode() ?? string.Empty;
        return Result<ChangeUsernameResponse>.Failure(errorCode);
    }

    public async Task<Result<GetProfileResponse>> GetAsync(CancellationToken cancellationToken)
    {
        AppendAuthorizationHeader();
        using var response = await _httpClient.GetAsync("api/profiles", cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<GetProfileResponse>(
            cancellationToken: cancellationToken
        );
        return Result<GetProfileResponse>.Success(result);
    }

    private void AppendAuthorizationHeader()
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;

        if (_tokenStorage.IsAuthenticated)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenStorage.AccessToken);
        }
    }
}
