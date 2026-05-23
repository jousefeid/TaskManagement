using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Common.Models;
using TaskManagement.Application.DTOs.Tasks;
using TaskManagement.Application.Features.Tasks.Commands;
using TaskManagement.Application.Features.Tasks.Queries;

namespace TaskManagement.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/projects/{projectId:guid}/tasks")]
[Authorize]
public class TasksController(IMediator mediator, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TaskResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(Guid projectId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetTasksQuery(projectId, currentUser.UserId), ct);
        return Ok(ApiResponse<IEnumerable<TaskResponse>>.Ok(result));
    }

    [HttpGet("{taskId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TaskResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid projectId, Guid taskId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetTaskByIdQuery(taskId, projectId, currentUser.UserId), ct);
        return Ok(ApiResponse<TaskResponse>.Ok(result));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TaskResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(Guid projectId, [FromBody] CreateTaskRequest request, CancellationToken ct)
    {
        var command = new CreateTaskCommand(
            projectId,
            currentUser.UserId,
            request.Title,
            request.Description,
            request.Priority,
            request.DueDate);

        var result = await mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById),
            new { projectId, taskId = result.Id },
            ApiResponse<TaskResponse>.Ok(result, "Task created."));
    }

    [HttpPut("{taskId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TaskResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid projectId, Guid taskId, [FromBody] UpdateTaskRequest request, CancellationToken ct)
    {
        var command = new UpdateTaskCommand(
            taskId,
            projectId,
            currentUser.UserId,
            request.Title,
            request.Description,
            request.Status,
            request.Priority,
            request.DueDate);

        var result = await mediator.Send(command, ct);
        return Ok(ApiResponse<TaskResponse>.Ok(result, "Task updated."));
    }

    [HttpDelete("{taskId:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid projectId, Guid taskId, CancellationToken ct)
    {
        await mediator.Send(new DeleteTaskCommand(taskId, projectId, currentUser.UserId), ct);
        return Ok(ApiResponse.OkNoData("Task deleted."));
    }
}
