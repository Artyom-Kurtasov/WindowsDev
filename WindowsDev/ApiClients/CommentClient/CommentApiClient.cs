using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using WindowsDev.Api.Common;
using WindowsDev.Api.DTO.Request.CommentsController;
using WindowsDev.Api.DTO.Response.CommentsController;
using WindowsDev.Application.Identity;
using WindowsDev.Application.Primitives;

namespace WindowsDev.ApiClients.CommentClient;

internal class CommentApiClient : ICommentApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ISecureTokenStorage _tokenStorage;

    public CommentApiClient(HttpClient httpClient, ISecureTokenStorage tokenStorage)
    {
        _httpClient = httpClient;
        _tokenStorage = tokenStorage;
    }

    public async Task<Result<GetCommentResponse>> GetCommentAsync(
        int id,
        CancellationToken cancellationToken
    )
    {
        AppendAuthorizationHeader();
        using var response = await _httpClient.GetAsync($"api/comments/{id}", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<GetCommentResponse>(
                cancellationToken: cancellationToken
            );
            return Result<GetCommentResponse>.Success(result);
        }

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        var errorCode = problem?.GetErrorCode() ?? string.Empty;
        return Result<GetCommentResponse>.Failure(errorCode);
    }

    public async Task<Result<List<GetCommentResponse>>> GetCommentsAsync(
        int taskId,
        CancellationToken cancellationToken
    )
    {
        AppendAuthorizationHeader();
        using var response = await _httpClient.GetAsync(
            $"api/comments/task/{taskId}",
            cancellationToken
        );

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<List<GetCommentResponse>>(
                cancellationToken: cancellationToken
            );
            return Result<List<GetCommentResponse>>.Success(result);
        }

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        var errorCode = problem?.GetErrorCode() ?? string.Empty;
        return Result<List<GetCommentResponse>>.Failure(errorCode);
    }

    public async Task<Result<AddCommentResponse>> AddCommentAsync(
        AddCommentRequest request,
        CancellationToken cancellationToken
    )
    {
        AppendAuthorizationHeader();
        using var response = await _httpClient.PostAsJsonAsync(
            "api/comments",
            request,
            cancellationToken
        );

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<AddCommentResponse>(
                cancellationToken: cancellationToken
            );
            return Result<AddCommentResponse>.Success(result);
        }

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        var errorCode = problem?.GetErrorCode() ?? string.Empty;
        return Result<AddCommentResponse>.Failure(errorCode);
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
