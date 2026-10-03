using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using WindowsDev.Api.Common;
using WindowsDev.Api.DTO.Request.AttachmentsController;
using WindowsDev.Api.DTO.Response.AttachmentsController;
using WindowsDev.Application.Identity;
using WindowsDev.Application.Primitives;

namespace WindowsDev.ApiClients.AttachmentClient;

internal class AttachmentApiClient : IAttachmentApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ISecureTokenStorage _tokenStorage;

    public AttachmentApiClient(HttpClient httpClient, ISecureTokenStorage tokenStorage)
    {
        _httpClient = httpClient;
        _tokenStorage = tokenStorage;
    }

    public async Task<Result<AddFileResponse>> AddFileAsync(
        AddFileRequest request,
        CancellationToken cancellationToken = default
    )
    {
        AppendAuthorizationHeader();
        using var response = await _httpClient.PostAsJsonAsync(
            "api/attachments",
            request,
            cancellationToken
        );

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<AddFileResponse>(
                cancellationToken: cancellationToken
            );
            return Result<AddFileResponse>.Success(result);
        }

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        var errorCode = problem?.GetErrorCode() ?? string.Empty;
        return Result<AddFileResponse>.Failure(errorCode);
    }

    public async Task<Result<List<GetAttachmentResponse>>> GetAttachmentsAsync(
        int taskId,
        CancellationToken cancellationToken = default
    )
    {
        AppendAuthorizationHeader();
        using var response = await _httpClient.GetAsync(
            $"api/attachments/task/{taskId}",
            cancellationToken
        );

        var result = await response.Content.ReadFromJsonAsync<List<GetAttachmentResponse>>(
            cancellationToken: cancellationToken
        );
        return Result<List<GetAttachmentResponse>>.Success(result);
    }

    public async Task OpenAttachmentAsync(
        string filePath,
        CancellationToken cancellationToken = default
    )
    {
        AppendAuthorizationHeader();
        using var response = await _httpClient.GetAsync(
            $"api/attachments/open?filePath={filePath}",
            cancellationToken
        );
        return;
    }

    public async Task<Result<GetAttachmentResponse>> GetAttachmentAsync(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        AppendAuthorizationHeader();
        using var response = await _httpClient.GetAsync($"api/attachments/{id}", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<GetAttachmentResponse>(
                cancellationToken: cancellationToken
            );
            return Result<GetAttachmentResponse>.Success(result);
        }

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        var errorCode = problem?.GetErrorCode() ?? string.Empty;
        return Result<GetAttachmentResponse>.Failure(errorCode);
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
