using System.Net.Http.Json;
using WindowsDev.Application.Common.Events;
using WindowsDev.Application.Identity.Cryptography;
using WindowsDev.Contracts.Api.Identity;

namespace WindowsDev.ApiClient;

internal class TokenRefreshHandler : DelegatingHandler
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ISecureTokenStorage _tokenStorage;
    private readonly ISessionEvent _sessionEvent;

    public TokenRefreshHandler(
        ISecureTokenStorage tokenStorage,
        IHttpClientFactory httpClientFactory,
        ISessionEvent sessionEvent
    )
    {
        _tokenStorage = tokenStorage;
        _httpClientFactory = httpClientFactory;
        _sessionEvent = sessionEvent;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var token = _tokenStorage.AccessToken;

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                token
            );
        }

        HttpResponseMessage response;
        response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var authClient = _httpClientFactory.CreateClient();

            var baseUri =
                request.RequestUri?.GetLeftPart(UriPartial.Authority) ?? "https://localhost:7145";

            var refreshTokenValue = _tokenStorage.RefreshToken;

            var refreshRequest = new RefreshTokenRequest { RefreshToken = refreshTokenValue };

            HttpResponseMessage refreshResponse;
            refreshResponse = await authClient.PostAsJsonAsync(
                $"{baseUri}/api/auth/refresh",
                refreshRequest,
                cancellationToken
            );

            using (refreshResponse)
            {
                if (!refreshResponse.IsSuccessStatusCode)
                {
                    _tokenStorage.Clear();
                    _sessionEvent.NotifySessionExpired();
                    return response;
                }

                UserLoginResponse? result;
                result = await refreshResponse.Content.ReadFromJsonAsync<UserLoginResponse>(
                    cancellationToken: cancellationToken
                );

                if (result != null)
                {
                    _tokenStorage.AccessToken = result.AccessToken;
                    _tokenStorage.RefreshToken = result.RefreshToken;

                    request.Headers.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue(
                            "Bearer",
                            result.AccessToken
                        );

                    response.Dispose();

                    var retryResponse = await base.SendAsync(request, cancellationToken);
                    return retryResponse;
                }
            }
        }

        return response;
    }
}
