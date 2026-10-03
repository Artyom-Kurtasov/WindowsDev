using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WindowsDev.Api.DTO.Request.AttachmentsController;
using WindowsDev.Api.DTO.Response.AttachmentsController;
using WindowsDev.Api.Logging;
using WindowsDev.Application.Tasks.Attachment;
using WindowsDev.Domain.Entities;

namespace WindowsDev.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class AttachmentsController : ApiControllerBase
{
    private readonly IMapper _mapper;
    private readonly ILogger<AttachmentsController> _logger;
    private readonly IAttachmentService _attachmentService;

    private const string GetAttachmentActionName = "GetAttachment";

    public AttachmentsController(
        IAttachmentService attachmentService,
        ILogger<AttachmentsController> logger,
        IMapper mapper
    )
    {
        _attachmentService = attachmentService;
        _logger = logger;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<ActionResult<AddFileResponse>> AddFileAsync([FromBody] AddFileRequest request)
    {
        var result = await _attachmentService.AddFile(request.FileName, request.TaskId);

        if (result.IsSuccess)
        {
            var response = _mapper.Map<AddFileResponse>(result.Value);
            AttachmentsControllerLogs.AddFileSuccessful(_logger, result.Value.Id, request.TaskId);
            return CreatedAtAction(GetAttachmentActionName, new { id = result.Value.Id }, response);
        }

        AttachmentsControllerLogs.FileNotSelected(_logger, request.TaskId);
        return CustomProblem(
            StatusCodes.Status400BadRequest,
            "File Not Selected",
            "File not selected.",
            result.Error
        );
    }

    [HttpGet("task/{taskId:int}")]
    public async Task<ActionResult<List<GetAttachmentResponse>>> GetAttachmentsAsync(
        [FromRoute] int taskId
    )
    {
        var result = await _attachmentService.GetAttachmentsAsync(taskId);
        var response = _mapper.Map<List<GetAttachmentResponse>>(result);

        AttachmentsControllerLogs.GetAttachmentsSuccessful(_logger, taskId);
        return Ok(response);
    }

    [HttpGet("open")]
    public async Task<IActionResult> OpenAttachmentAsync([FromQuery] string filePath)
    {
        await _attachmentService.OpenFile(filePath);

        AttachmentsControllerLogs.OpenAttachmentSuccessful(_logger, filePath);
        return Ok();
    }

    [HttpGet("{id:int}")]
    [ActionName(GetAttachmentActionName)]
    public async Task<ActionResult<GetAttachmentResponse>> GetAttachmentAsync([FromRoute] int id)
    {
        var result = await _attachmentService.GetAttachmentAsync(id);

        if (!result.IsSuccess)
        {
            AttachmentsControllerLogs.AttachmentNotFound(_logger, id);
            return CustomProblem(
                StatusCodes.Status404NotFound,
                "Attachment Not Found",
                $"Attachment with id {id} not found.",
                result.Error
            );
        }

        var response = _mapper.Map<GetAttachmentResponse>(result.Value);

        AttachmentsControllerLogs.GetAttachmentSuccessful(_logger, id);
        return Ok(response);
    }
}
