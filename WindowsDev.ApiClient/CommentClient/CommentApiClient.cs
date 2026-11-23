using AutoMapper;
using System.Net.Http.Json;
using WindowsDev.Application.Identity.Cryptography;
using WindowsDev.Application.Primitives;
using WindowsDev.Contracts.Api.Comments;
using WindowsDev.Contracts.Api.Common;
using WindowsDev.Contracts.UI.Comments;

namespace WindowsDev.ApiClient.CommentClient;

public class CommentApiClient : ApiClientBase, ICommentApiClient
{
    private readonly IMapper _mapper;

    public CommentApiClient(HttpClient httpClient, ISecureTokenStorage tokenStorage, IMapper mapper) : base(httpClient, tokenStorage)
    {
        _mapper = mapper;
    }

    public async Task<Result<CommentInfo>> GetCommentAsync(
        int id,
        CancellationToken cancellationToken
    )
    {
        await AppendAuthorizationHeaderAsync(cancellationToken);
        using var response = await _httpClient.GetAsync($"api/comments/{id}", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<GetCommentResponse>(
                cancellationToken: cancellationToken
            );

            var info = _mapper.Map<CommentInfo>(result);
            return Result<CommentInfo>.Success(info);
        }

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return Result<CommentInfo>.Failure("Unauthorized"); // Change string!
        }

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        var errorCode = problem?.GetErrorCode() ?? string.Empty;
        return Result<CommentInfo>.Failure(errorCode);
    }

    public async Task<Result<CommentInfo>> AddCommentAsync(
        AddCommentForm request,
        CancellationToken cancellationToken
    )
    {
        await AppendAuthorizationHeaderAsync(cancellationToken);
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

            var info = _mapper.Map<CommentInfo>(result);
            return Result<CommentInfo>.Success(info);
        }

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        var errorCode = problem?.GetErrorCode() ?? string.Empty;
        return Result<CommentInfo>.Failure(errorCode);
    }

    public async Task<Result<List<CommentInfo>>> GetCommentsAsync(
        int taskId,
        CancellationToken cancellationToken
    )
    {
        await AppendAuthorizationHeaderAsync(cancellationToken);
        using var response = await _httpClient.GetAsync(
            $"api/comments/task/{taskId}",
            cancellationToken
        );

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<List<GetCommentResponse>>(
                cancellationToken: cancellationToken
            );
            var info = _mapper.Map<List<CommentInfo>>(result);
            return Result<List<CommentInfo>>.Success(info);
        }

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        var errorCode = problem?.GetErrorCode() ?? string.Empty;
        return Result<List<CommentInfo>>.Failure(errorCode);
    }
}
