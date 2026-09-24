using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using WindowsDev.Api.DTO.Request.AttachmentsController;
using WindowsDev.Api.DTO.Response.AttachmentsController;
using WindowsDev.Application.Tasks.Attachment;

namespace WindowsDev.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttachmentsController : ControllerBase
    {
        private readonly ILogger<AttachmentsController> _logger;
        private readonly IAttachmentService _attacmentService;

        public AttachmentsController(IAttachmentService attacmentService, 
            ILogger<AttachmentsController> logger)
        {
            _attacmentService = attacmentService;
            _logger = logger;
        }

        [Authorize]
        [HttpPost("AddFile")]
        public async Task<ActionResult<AddFileResponse>> AddFileAsync(AddFileRequest request)
        {
            var result = await _attacmentService.AddFile(request.TaskId);

            var response = new AddFileResponse
            {
                FileExtension = result.Value.FileExtension,
                FileName = result.Value.FileName,
                FilePath = result.Value.FilePath,
                FileSize = result.Value.FileSize,
                TaskId = result.Value.TaskId
            };

            return CreatedAtAction(nameof(GetAttachmentAsync), new { id = result.Value.Id }, response);
        }

        [Authorize]
        [HttpGet("GetAttachments")]
        public async Task<ActionResult<List<GetAttachmentResponse>>> GetAttachmentsAsync(int id)
        {
            var result = await _attacmentService.GetAttachmentsAsync(id);

            var response = new List<GetAttachmentResponse>();

            int countOfAttachments = result.Value.Count;

            for (int i = 0;  i < countOfAttachments; i++)
            {
                var item = new GetAttachmentResponse
                {
                    FileExtension = result.Value[i].FileExtension,
                    FileName = result.Value[i].FileName,
                    FilePath = result.Value[i].FilePath,
                    FileSize = result.Value[i].FileSize,
                    TaskId = result.Value[i].TaskId
                };

                response.Add(item);
            }

            return Ok(response);
        }

        [Authorize]
        [HttpGet("OpenAttachment")]
        public async Task<IActionResult> OpenAttachmentAsync(OpenAttachmentRequest request)
        {
            await _attacmentService.OpenFile(request.FilePath);
            return Ok();
        }

        [Authorize]
        [HttpGet("{id:int}")]
        [ActionName("GetAttachmentAsync")]
        public async Task<ActionResult<GetAttachmentResponse>> GetAttachmentAsync(int id)
        {
            var attachment = _attacmentService.GetAttachmentAsync(id);

            var response = new GetAttachmentResponse
            {
                FileExtension = attachment.Result.Value.FileExtension,
                FileName = attachment.Result.Value.FileName,
                FilePath = attachment.Result.Value.FilePath,
                FileSize = attachment.Result.Value.FileSize,
                TaskId = attachment.Result.Value.TaskId
            };

            return Ok(response);
        }
    }
}
