using AutoMapper;
using System.Net.Http.Json;
using WindowsDev.Application.Identity.Cryptography;
using WindowsDev.Application.Primitives;
using WindowsDev.Contracts.Api.Common;
using WindowsDev.Contracts.Api.Profiles;
using WindowsDev.Contracts.UI.Profiles;

namespace WindowsDev.ApiClient.ProfileClient;

public class ProfileApiClient : ApiClientBase, IProfileApiClient
{
    private readonly IMapper _mapper;

    public ProfileApiClient(HttpClient httpClient, ISecureTokenStorage tokenStorage, IMapper mapper)
        : base(httpClient, tokenStorage)
    {
        _mapper = mapper;
    }

    public async Task<Result<ChangePasswordInfo>> ChangePasswordAsync(
        ChangePasswordForm request,
        CancellationToken cancellationToken
    )
    {
        await AppendAuthorizationHeaderAsync(cancellationToken);
        var apiRequest = _mapper.Map<ChangePasswordRequest>(request);
        using var response = await _httpClient.PostAsJsonAsync(
            "api/profiles/changePassword",
            apiRequest,
            cancellationToken
        );

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ChangePasswordResponse>(
                cancellationToken: cancellationToken
            );

            var info = _mapper.Map<ChangePasswordInfo>(result);
            return Result<ChangePasswordInfo>.Success(info);
        }

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        var errorCode = problem?.GetErrorCode() ?? string.Empty;
        return Result<ChangePasswordInfo>.Failure(errorCode);
    }

    public async Task<Result<ChangeUsernameInfo>> ChangeUsernameAsync(
        ChangeUsernameForm request,
        CancellationToken cancellationToken
    )
    {
        await AppendAuthorizationHeaderAsync(cancellationToken);
        var apiRequest = _mapper.Map<ChangeUsernameRequest>(request);

        using var response = await _httpClient.PostAsJsonAsync(
            "api/profiles/changeUsername",
            apiRequest,
            cancellationToken
        );

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ChangeUsernameResponse>(
                cancellationToken: cancellationToken
            );

            var info = _mapper.Map<ChangeUsernameInfo>(result);
            return Result<ChangeUsernameInfo>.Success(info);
        }

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        var errorCode = problem?.GetErrorCode() ?? string.Empty;
        return Result<ChangeUsernameInfo>.Failure(errorCode);
    }

    public async Task<Result<ProfileInfo>> GetAsync(CancellationToken cancellationToken)
    {
        await AppendAuthorizationHeaderAsync(cancellationToken);
        using var response = await _httpClient.GetAsync("api/profiles", cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<GetProfileResponse>(
            cancellationToken: cancellationToken
        );

        var info = _mapper.Map<ProfileInfo>(result);
        return Result<ProfileInfo>.Success(info);
    }
}
