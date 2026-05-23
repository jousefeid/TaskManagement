using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Common.Models;
using TaskManagement.Application.DTOs.Projects;
using TaskManagement.Application.Features.Projects.Commands;
using TaskManagement.Application.Features.Projects.Queries;

namespace TaskManagement.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize] 
public class ProjectsController(IMediator mediator, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProjectResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new GetProjectsQuery(currentUser.UserId), ct);
        return Ok(ApiResponse<IEnumerable<ProjectResponse>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProjectResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetProjectByIdQuery(id, currentUser.UserId), ct);
        return Ok(ApiResponse<ProjectResponse>.Ok(result));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProjectResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProjectRequest request, CancellationToken ct)
    {
        var command = new CreateProjectCommand(request.Name, request.Description, currentUser.UserId);
        var result = await mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ProjectResponse>.Ok(result, "Project created."));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProjectResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectRequest request, CancellationToken ct)
    {
        var command = new UpdateProjectCommand(id, request.Name, request.Description, currentUser.UserId);
        var result = await mediator.Send(command, ct);
        return Ok(ApiResponse<ProjectResponse>.Ok(result, "Project updated."));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteProjectCommand(id, currentUser.UserId), ct);
        return Ok(ApiResponse.OkNoData("Project deleted."));
    }
}
