using System.Net.Http.Headers;
using WindowsDev.Application.Identity.Cryptography;

namespace WindowsDev.ApiClient;

public class ApiClientBase
{
    protected readonly ISecureTokenStorage _tokenStorage;
    protected readonly HttpClient _httpClient;

    public ApiClientBase(HttpClient httpClient, ISecureTokenStorage tokenStorage)
    {
        _httpClient = httpClient;
        _tokenStorage = tokenStorage;
    }

    protected async Task AppendAuthorizationHeaderAsync(
        CancellationToken cancellationToken = default
    )
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _tokenStorage.AccessToken
        );
    }
}
