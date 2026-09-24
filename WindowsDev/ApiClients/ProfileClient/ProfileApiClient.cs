using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using WindowsDev.Api.Common;
using WindowsDev.Api.DTO.Request.ProfilesController;
using WindowsDev.Api.DTO.Response.ProfilesController;
using WindowsDev.Application.Primitives;

namespace WindowsDev.ApiClients.ProfileClient;

internal class ProfileApiClient : IProfileApiClient
{
    private readonly HttpClient _httpClient;

    public ProfileApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result<ProfileChangePasswordResponse>> ChangePasswordAsync(
        ProfileChangePasswordRequest request,
        CancellationToken cancellationToken
    )
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "api/profiles/changePassword",
            request,
            cancellationToken
        );

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ProfileChangePasswordResponse>(
                cancellationToken: cancellationToken
            );
            return Result<ProfileChangePasswordResponse>.Success(result);
        }

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        var errorCode = problem?.GetErrorCode() ?? string.Empty;
        return Result<ProfileChangePasswordResponse>.Failure(errorCode);
    }

    public async Task<Result<UsernameChangeResponse>> ChangeUsernameAsync(
        UsernameChangeRequest request,
        CancellationToken cancellationToken
    )
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "api/profiles/changeUsername",
            request,
            cancellationToken
        );

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<UsernameChangeResponse>(
                cancellationToken: cancellationToken
            );

            return Result<UsernameChangeResponse>.Success(result);
        }

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        var errorCode = problem?.GetErrorCode() ?? string.Empty;
        return Result<UsernameChangeResponse>.Failure(errorCode);
    }

    public async Task<Result<GetAsyncResponse>> GetAsync(CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync("api/profiles", cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<GetAsyncResponse>(
            cancellationToken: cancellationToken
        );
        return Result<GetAsyncResponse>.Success(result);
    }
}
