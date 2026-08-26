using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using WindowsDev.Api.DTOs.Request;
using WindowsDev.Api.DTOs.Response;
using WindowsDev.Application.Primitives;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;

namespace WindowsDev.ApiClients.AuthClient;

internal sealed class AuthApiClient : IAuthApiClient
{
    private readonly HttpClient _httpClient;

    public AuthApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result<UserRegisterResponse>> RegisterAsync(
        UserRegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "api/auth/register",
            request,
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            return Result<UserRegisterResponse>.Failure(
                AuthErrors.RegistrationFailed);
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            return Result<UserRegisterResponse>.Failure(
                AuthErrors.RegistrationFailed);
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Registration API returned HTTP {(int)response.StatusCode} ({response.ReasonPhrase}).");
        }

        var result = await response.Content.ReadFromJsonAsync<UserRegisterResponse>(
            cancellationToken: cancellationToken);

        if (result is null)
        {
            throw new InvalidOperationException(
                "Registration API returned an empty or invalid response body.");
        }

        return Result<UserRegisterResponse>.Success(result);
    }
}