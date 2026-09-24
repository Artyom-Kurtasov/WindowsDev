using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WindowsDev.Api.DTO.Request.TasksController;
using WindowsDev.Api.DTO.Response.ProjectsController;
using WindowsDev.Api.DTO.Response.TasksController;
using WindowsDev.Application.Tasks;
using WindowsDev.Domain.Entities;
using WindowsDev.Domain.Messages.DialogsMessages.Errors;
using TaskStatus = WindowsDev.Domain.Enums.TaskStatus;

namespace WindowsDev.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class TasksController : ApiControllerBase
{
    private readonly ITaskService _taskService;
    private readonly IMapper _mapper;

    public TasksController(ITaskService taskService, IMapper mapper)
    {
        _taskService = taskService;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<ActionResult<AddResponse>> AddAsync(AddTaskRequest request)
    {
        var entity = _mapper.Map<TasksInfo>(request);
        entity.CreatedAt = DateTime.Now.ToUniversalTime();

        await _taskService.AddAsync(entity);

        var response = _mapper.Map<AddTaskResponse>(entity);

        return CreatedAtAction(nameof(GetTaskByIdAsync), new { id = entity.Id }, response);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync([FromQuery] int id)
    {
        await _taskService.DeleteAsync(id);
        return NoContent();
    }

    [HttpPatch("{id:int}")]
    public async Task<ActionResult<UpdateTaskResponse>> UpdateAsync(
        [FromQuery] int id,
        [FromBody] UpdateTaskRequest request
    )
    {
        var task = await _taskService.GetAsync(id);
        if (task is null)
            return CustomProblem(
                StatusCodes.Status404NotFound,
                "Task Not Found",
                $"The requested task (ID: {id}) does not exist. Could it have been deleted?",
                TaskErrors.TaskNotFound
            );

        _mapper.Map(request, task);
        await _taskService.UpdateAsync(task);

        var response = _mapper.Map<UpdateTaskResponse>(task);
        return Ok(response);
    }

    [HttpGet]
    public async Task<ActionResult<List<GetTasksResponse>>> GetTasksAsync(
        [FromQuery] int projectId,
        [FromQuery] string? search,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        [FromQuery] List<TaskStatus> statuses
    )
    {
        var entity = new TaskFilter
        {
            ProjectId = projectId,
            Seacrh = search,
            Page = page,
            PageSize = pageSize,
            Statuses = statuses,
        };

        var result = await _taskService.GetTasksAsync(entity);
        var response = _mapper.Map<List<GetTasksResponse>>(result);
        return Ok(response);
    }

    [HttpGet("count")]
    public async Task<ActionResult<GetTasksCountResponse>> GetTasksCountAsync(
        [FromQuery] int projectId
    )
    {
        var result = await _taskService.GetTasksCountAsync(projectId);
        var response = new GetTasksCountResponse { TotalCount = result };

        return Ok(response);
    }

    [HttpGet("{id:int}")]
    [ActionName("GetTaskByIdAsync")]
    public async Task<ActionResult<GetTaskByIdResponse>> GetTaskByIdAsync(int id)
    {
        var task = await _taskService.GetAsync(id);
        if (task is null)
            return CustomProblem(
                StatusCodes.Status404NotFound,
                "Task Not Found",
                $"The requested task (ID: {id}) does not exist. Could it have been deleted?",
                TaskErrors.TaskNotFound
            );

        var response = _mapper.Map<GetTaskByIdResponse>(task);
        return Ok(response);
    }
}
