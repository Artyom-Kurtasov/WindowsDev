using AutoMapper;
using System.Net.Http.Json;
using WindowsDev.Application.Identity.Cryptography;
using WindowsDev.Application.Primitives;
using WindowsDev.Contracts.Api.Attachments;
using WindowsDev.Contracts.Api.Common;
using WindowsDev.Contracts.UI.Tasks;

namespace WindowsDev.ApiClient.AttachmentClient;

public class AttachmentApiClient : IAttachmentApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ISecureTokenStorage _tokenStorage;
    private readonly IMapper _mapper;

    public AttachmentApiClient(HttpClient httpClient, ISecureTokenStorage tokenStorage, IMapper mapper)
    {
        _httpClient = httpClient;
        _tokenStorage = tokenStorage;
        _mapper = mapper;
    }

    public async Task<Result<AttachmentInfo>> AddFileAsync(
        AttachmentForm request,
        CancellationToken cancellationToken = default
    )
    {
        AppendAuthorizationHeader();

        var attachment = _mapper.Map<AddAttachmentRequest>(request);
        using var response = await _httpClient.PostAsJsonAsync(
            "api/attachments",
            attachment,
            cancellationToken
        );

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<AddAttachmentResponse>(
                cancellationToken: cancellationToken
            );

            var attachmentInfo = _mapper.Map<AttachmentInfo>(result);
            return Result<AttachmentInfo>.Success(attachmentInfo);
        }
        
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        var errorCode = problem?.GetErrorCode() ?? string.Empty;
        return Result<AttachmentInfo>.Failure(errorCode);
    }

    public async Task<Result<List<AttachmentInfo>>> GetAttachmentsAsync(
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

        var attachmentsInfo = _mapper.Map<List<AttachmentInfo>>(result);
        return Result<List<AttachmentInfo>>.Success(attachmentsInfo);
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

    public async Task<Result<AttachmentInfo>> GetAttachmentAsync(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        AppendAuthorizationHeader();
        using var response = await _httpClient.GetAsync(
            $"api/attachments/{id}",
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<GetAttachmentResponse>(
                cancellationToken: cancellationToken
            );

            var attachmentInfo = _mapper.Map<AttachmentInfo>(result);
            return Result<AttachmentInfo>.Success(attachmentInfo);
        }

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        var errorCode = problem?.GetErrorCode() ?? string.Empty;
        return Result<AttachmentInfo>.Failure(errorCode);
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
