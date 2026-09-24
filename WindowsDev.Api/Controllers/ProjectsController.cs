using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WindowsDev.Api.DTO.Request.ProjectsController;
using WindowsDev.Api.DTO.Request.ProjectService;
using WindowsDev.Api.DTO.Response.ProjectsController;
using WindowsDev.Api.Logging;
using WindowsDev.Application.Identity;
using WindowsDev.Application.Projects;
using WindowsDev.Domain.Entities;

namespace WindowsDev.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly ILogger<ProjectsController> _logger;
    private readonly IMapper _mapper;
    private readonly IUserSession _userSession;

    public ProjectsController(
        IProjectService projectService,
        ILogger<ProjectsController> logger,
        IMapper mapper,
        IUserSession userSession
    )
    {
        _projectService = projectService;
        _logger = logger;
        _mapper = mapper;
        _userSession = userSession;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<AddResponse>> AddAsync([FromBody] AddProjectRequest request)
    {
        var entity = _mapper.Map<ProjectsInfo>(request);
        entity.UserId = _userSession.UserId;
        entity.CreatedAt = DateTime.UtcNow;

        await _projectService.AddAsync(entity);

        var response = _mapper.Map<AddResponse>(entity);

        ProjectsControllerLogs.AddSuccessful(_logger, _userSession.Login, entity.Id);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync([FromRoute] int id)
    {
        await _projectService.DeleteAsync(id);

        ProjectsControllerLogs.DeleteSuccessful(_logger);
        return NoContent();
    }

    [Authorize]
    [HttpPatch("{id:int}")]
    public async Task<ActionResult<UpdateResponse>> UpdateAsync(
        [FromRoute] int id,
        [FromBody] UpdateProjectRequest request
    )
    {
        var project = await _projectService.GetProjectById(id);
        if (project is null)
        {
            ProjectsControllerLogs.ProjectNotFound(_logger, id);
            return NotFound();
        }

        _mapper.Map(request, project);

        await _projectService.UpdateAsync(project);

        var response = _mapper.Map<UpdateResponse>(project);

        ProjectsControllerLogs.UpdateSuccessful(_logger, response.Id);
        return Ok(response);
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<GetProjectsResponse>>> GetProjectsAsync(
        [FromQuery] int page,
        [FromQuery] int pageSize,
        [FromQuery] string? searchFilter
    )
    {

        var result = await _projectService.GetProjectsAsync(
            _userSession.UserId,
            page,
            pageSize,
            searchFilter ?? ""
            );

        var response = _mapper.Map<List<GetProjectsResponse>>(result);

        ProjectsControllerLogs.GetProjectsSuccessful(
            _logger,
            page,
            pageSize,
            searchFilter ?? "",
            _userSession.UserId
        );

        return Ok(response);
       
    }

    [Authorize]
    [HttpGet("count")]
    public async Task<ActionResult<GetProjectsCountResponse>> GetCountAsync()
    { 
        var result = await _projectService.GetProjectsCountAsync(_userSession.UserId);

        var response = new GetProjectsCountResponse { TotalCount = result };

        ProjectsControllerLogs.GetProjectsCountSuccessful(_logger, _userSession.UserId);
        return Ok(response);
    }

    [Authorize]
    [HttpGet("{id:int}")]
    [ActionName(nameof(GetByIdAsync))]
    public async Task<ActionResult<GetByIdResponse>> GetByIdAsync(int id)
    {
        var project = await _projectService.GetProjectById(id);
        if (project is null)
            return NotFound();

        var response = _mapper.Map<GetByIdResponse>(project);

        ProjectsControllerLogs.GetProjectSuccessful(_logger, id);
        return Ok(response);
    }
}
