using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WindowsDev.Api.DTO.Request.CommentsController;
using WindowsDev.Api.DTO.Response.CommentsController;
using WindowsDev.Api.Logging;
using WindowsDev.Application.Tasks.Comment;

namespace WindowsDev.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class CommentsController : ApiControllerBase
{
    private readonly IMapper _mapper;
    private readonly ICommentService _commentService;
    private readonly ILogger<CommentsController> _logger;

    public CommentsController(
        ICommentService commentService,
        IMapper mapper,
        ILogger<CommentsController> logger
    )
    {
        _commentService = commentService;
        _mapper = mapper;
        _logger = logger;
    }

    private const string GetCommentActionName = "GetComment";

    [HttpGet("{id:int}")]
    [ActionName(GetCommentActionName)]
    public async Task<ActionResult<GetCommentResponse>> GetCommentAsync([FromRoute] int id)
    {
        var result = await _commentService.GetCommentAsync(id);

        if (result.IsSuccess)
        {
            var response = _mapper.Map<GetCommentResponse>(result.Value);
            CommentsControllerLogs.GetCommentSuccessful(_logger, id);
            return Ok(response);
        }

        CommentsControllerLogs.CommentNotFound(_logger, id);
        return CustomProblem(
            StatusCodes.Status404NotFound,
            "Comment Not Found",
            $"Comment with {id} not found.",
            result.Error
        );
    }

    [HttpGet("task/{taskId:int}")]
    public async Task<ActionResult<List<GetCommentResponse>>> GetCommentsAsync(
        [FromRoute] int taskId
    )
    {
        var result = await _commentService.GetCommentsAsync(taskId);
        var response = _mapper.Map<List<GetCommentResponse>>(result);

        CommentsControllerLogs.GetCommentsSuccessful(_logger, taskId);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<AddCommentResponse>> AddCommentAsync(
        [FromBody] AddCommentRequest request
    )
    {
        var result = await _commentService.AddCommentAsync(request.TaskId, request.CommentText);
        var response = _mapper.Map<AddCommentResponse>(result);

        CommentsControllerLogs.AddCommentSuccessful(_logger, result.Id, request.TaskId);
        return CreatedAtAction(GetCommentActionName, new { id = result.Id }, response);
    }
}
