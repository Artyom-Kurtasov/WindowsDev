using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WindowsDev.Api.DTO.Request.CommentsController;
using WindowsDev.Api.DTO.Response.CommentsController;
using WindowsDev.Application.Tasks.Comment;

namespace WindowsDev.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [Authorize]
    [HttpGet("GetComment")]
    [ActionName("GetComment")]
    public async Task<ActionResult<GetCommentResponse>> GetCommentAsync(int id)
    {
        var result = await _commentService.GetCommentAsync(id);

        if (result is not null) 
        {
            var response = new GetCommentResponse
            {
                Id = result.Id,
                Author = result.Author,
                Text = result.Text,
                CreatedAt = result.CreatedAt,
                TaskId = result.TaskId
            };

            return Ok(response);
        }

        return NotFound();
    }

    [Authorize]
    [HttpGet("GetComments")]
    public async Task<ActionResult<List<GetCommentResponse>>> GetCommentsAsync(int taskId)
    {
        var result = await _commentService.GetCommentsAsync(taskId);

        var response = new List<GetCommentResponse>();

        for(int i = 0; i < result.Count; i++)
        {
            var item = new GetCommentResponse
            {
                Id = result[i].Id,
                Author = result[i].Author,
                TaskId = result[i].TaskId,
                Text = result[i].Text,
                CreatedAt = result[i].CreatedAt
            };

            response.Add(item);
        }

        return Ok(response);
    }

    [Authorize]
    [HttpPost("AddComment")]
    public async Task<ActionResult<AddCommentResponse>> AddCommentAsync(AddCommentRequest request)
    {
        var result = await _commentService.AddCommentAsync(request.TaskId, request.CommentText);

        var response = new AddCommentResponse
        {
            Id = result.Value.Id,
            TaskId = result.Value.TaskId,
            Text = result.Value.Text,
            Author = result.Value.Author,
            CreatedAt = result.Value.CreatedAt
        };

        return CreatedAtAction(nameof(GetCommentAsync), new { id = result.Value.Id }, response);
    }
}
