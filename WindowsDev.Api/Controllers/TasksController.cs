using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WindowsDev.Api.Logging;
using WindowsDev.Application.Tasks;
using WindowsDev.Contracts.Api.Tasks;
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
    private readonly ILogger<TasksController> _logger;

    public TasksController(
        ITaskService taskService,
        IMapper mapper,
        ILogger<TasksController> logger
    )
    {
        _taskService = taskService;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<AddTaskResponse>> AddAsync(AddTaskRequest request)
    {
        var entity = _mapper.Map<TasksInfo>(request);
        entity.CreatedAt = DateTime.Now.ToUniversalTime();

        await _taskService.AddAsync(entity);

        var response = _mapper.Map<AddTaskResponse>(entity);

        TasksControllerLogs.AddSuccessful(_logger, entity.Id, entity.ProjectId);
        return CreatedAtAction(nameof(GetTaskByIdAsync), new { id = entity.Id }, response);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync([FromRoute] int id)
    {
        await _taskService.DeleteAsync(id);

        TasksControllerLogs.DeleteSuccessful(_logger, id);
        return NoContent();
    }

    [HttpPatch("{id:int}")]
    public async Task<ActionResult<UpdateTaskResponse>> UpdateAsync(
        [FromRoute] int id,
        [FromBody] UpdateTaskRequest request
    )
    {
        var task = await _taskService.GetAsync(id);
        if (task is null)
        {
            TasksControllerLogs.TaskNotFound(_logger, id);
            return CustomProblem(
                StatusCodes.Status404NotFound,
                "Task Not Found",
                $"The requested task (ID: {id}) does not exist. Could it have been deleted?",
                TaskErrors.TaskNotFound
            );
        }

        _mapper.Map(request, task);
        await _taskService.UpdateAsync(task);

        var response = _mapper.Map<UpdateTaskResponse>(task);

        TasksControllerLogs.UpdateSuccessful(_logger, id);
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
        var response = result.Value.Select(task => _mapper.Map<GetTasksResponse>(task)).ToList();

        TasksControllerLogs.GetTasksSuccessful(_logger, projectId, page, pageSize, search);
        return Ok(response);
    }

    [HttpGet("count")]
    public async Task<ActionResult<GetTasksCountResponse>> GetTasksCountAsync(
        [FromQuery] int projectId
    )
    {
        var result = await _taskService.GetTasksCountAsync(projectId);
        var response = new GetTasksCountResponse { TotalCount = result };

        TasksControllerLogs.GetTasksCountSuccessful(_logger, projectId);
        return Ok(response);
    }

    [HttpGet("{id:int}")]
    [ActionName("GetTaskByIdAsync")]
    public async Task<ActionResult<GetTaskByIdResponse>> GetTaskByIdAsync(int id)
    {
        var task = await _taskService.GetAsync(id);
        if (task is null)
        {
            TasksControllerLogs.TaskNotFound(_logger, id);
            return CustomProblem(
                StatusCodes.Status404NotFound,
                "Task Not Found",
                $"The requested task (ID: {id}) does not exist. Could it have been deleted?",
                TaskErrors.TaskNotFound
            );
        }

        var response = _mapper.Map<GetTaskByIdResponse>(task);

        TasksControllerLogs.GetTaskSuccessful(_logger, id);
        return Ok(response);
    }
}
