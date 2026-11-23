using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WindowsDev.Api.Attachments;
using WindowsDev.Api.Logging;
using WindowsDev.Api.ProblemDetails;
using WindowsDev.Api.Urls;
using WindowsDev.Contracts.Api.Attachments;

namespace WindowsDev.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class AttachmentsController : ApiControllerBase
{
    private readonly ILogger<AttachmentsController> _logger;
    private readonly IAttachmentService _attachmentService;

    private const string DownloadAttachmentActionName = "DownloadAttachment";

    public AttachmentsController(
        IAttachmentService attachmentService,
        ILogger<AttachmentsController> logger
    )
    {
        _attachmentService = attachmentService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> UploadFileAsync([FromForm] IFormFile file)
    {
        var result = await _attachmentService.UploadFileAsync(file);

        if (!result.IsSuccess)
        {
            AttachmentsControllerLogs.FileNotSelected(_logger, file?.FileName ?? "<null>");
            return CustomProblem(
                StatusCodes.Status400BadRequest,
                AttachmentProblemDetails.FileDidNotSelect,
                AttachmentProblemDetails.FileDidNotSelectDetail,
                result.Error
            );
        }

        var safeName = result.Value;
        AttachmentsControllerLogs.UploadFileSuccessful(_logger, safeName);

        var response = new UploadFileResponse(safeName);
        return CreatedAtAction(DownloadAttachmentActionName, new { fileName = safeName }, response);
    }

    [HttpGet]
    public ActionResult<List<FileListItemResponse>> GetFiles()
    {
        var result = _attachmentService.GetFiles();

        if (!result.IsSuccess)
        {
            AttachmentsControllerLogs.GetFilesFailed(_logger, result.Error);
            return CustomProblem(
                StatusCodes.Status500InternalServerError,
                AttachmentProblemDetails.UploadsFolderUnavailable,
                AttachmentProblemDetails.UploadsFolderUnavailableDetail,
                result.Error
            );
        }

        AttachmentsControllerLogs.GetFilesSuccessful(_logger, result.Value.Count);
        return Ok(result.Value);
    }

    [HttpGet(AttachmentUrls.DownloadFileUrl)]
    [ActionName(DownloadAttachmentActionName)]
    public IActionResult DownloadFile([FromQuery] string fileName)
    {
        var result = _attachmentService.DownloadFile(fileName);

        if (!result.IsSuccess)
        {
            AttachmentsControllerLogs.AttachmentNotFound(_logger, fileName);
            return CustomProblem(
                StatusCodes.Status404NotFound,
                AttachmentProblemDetails.AttachmentNotFound,
                AttachmentProblemDetails.AttachmentNotFoundDetail(fileName),
                result.Error
            );
        }

        var dto = result.Value;
        AttachmentsControllerLogs.DownloadFileSuccessful(_logger, dto.FileName);
        return File(dto.Stream, dto.ContentType, dto.FileName);
    }
}