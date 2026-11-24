using AutoMapper;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using WindowsDev.Application.Identity.Cryptography;
using WindowsDev.Application.Primitives;
using WindowsDev.Contracts.Api.Attachments;
using WindowsDev.Contracts.Api.Common;
using WindowsDev.Contracts.UI.Attachments;

namespace WindowsDev.ApiClient.AttachmentClient;

public class AttachmentApiClient : ApiClientBase, IAttachmentApiClient
{
    private readonly IMapper _mapper;

    public AttachmentApiClient(
        HttpClient httpClient,
        ISecureTokenStorage tokenStorage,
        IMapper mapper
    )
        : base(httpClient, tokenStorage)
    {
        _mapper = mapper;
    }

    public async Task<Result<string>> UploadFileAsync(
        UploadFileForm request,
        int taskId,
        CancellationToken cancellationToken = default
    )
    {
        await AppendAuthorizationHeaderAsync(cancellationToken);

        using var multipart = new MultipartFormDataContent();
        var fileContent = new StreamContent(request.Content);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(request.ContentType);
        multipart.Add(fileContent, "file", request.FileName);

        using var response = await _httpClient.PostAsync(
            $"api/attachments/{taskId}",
            multipart,
            cancellationToken
        );

        if (response.IsSuccessStatusCode)
        {
            var payload = await response.Content.ReadFromJsonAsync<UploadFileResponse>(
                cancellationToken: cancellationToken
            );

            return Result<string>.Success(payload?.FileName ?? string.Empty);
        }

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        var errorCode = problem?.GetErrorCode() ?? string.Empty;
        return Result<string>.Failure(errorCode);
    }

    public async Task<Result<List<FileItem>>> GetFilesAsync(
        int taskId,
        CancellationToken cancellationToken = default
    )
    {
        await AppendAuthorizationHeaderAsync(cancellationToken);

        using var response = await _httpClient.GetAsync($"api/attachments/{taskId}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(
                cancellationToken
            );
            var errorCode = problem?.GetErrorCode() ?? string.Empty;
            return Result<List<FileItem>>.Failure(errorCode);
        }

        var files = await response.Content.ReadFromJsonAsync<List<FileListItemResponse>>(
            cancellationToken: cancellationToken
        );

        var result = _mapper.Map<List<FileItem>>(files);
        return Result<List<FileItem>>.Success(result);
    }

    public async Task<Result<FileDownloadInfo>> DownloadFileAsync(
    string fileName,
    CancellationToken cancellationToken = default
)
    {
        await AppendAuthorizationHeaderAsync(cancellationToken);

        var url = $"api/attachments/download?fileName={Uri.EscapeDataString(fileName)}";

        using var response = await _httpClient.GetAsync(
            url,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken
        );

        if (!response.IsSuccessStatusCode)
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(
                cancellationToken
            );
            var errorCode = problem?.GetErrorCode() ?? string.Empty;
            return Result<FileDownloadInfo>.Failure(errorCode);
        }

        var contentType =
            response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

        var memoryStream = new MemoryStream();
        await response.Content.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        var result = new FileDownloadInfo(memoryStream, contentType, fileName);
        return Result<FileDownloadInfo>.Success(result);
    }

}
