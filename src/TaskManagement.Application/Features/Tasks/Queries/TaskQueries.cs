using MediatR;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.DTOs.Tasks;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Features.Tasks.Queries;

internal static class TaskQueryMapper
{
    public static TaskResponse ToResponse(ProjectTask task) => new(
        task.Id,
        task.Title,
        task.Description,
        task.Status.ToString(),
        task.Priority.ToString(),
        task.DueDate,
        task.ProjectId,
        task.CreatedAt);
}

// ════════════════════════════════════════════════════════════
//  GET ALL TASKS FOR PROJECT
// ════════════════════════════════════════════════════════════

public record GetTasksQuery(Guid ProjectId, Guid UserId) : IRequest<IEnumerable<TaskResponse>>;

public class GetTasksQueryHandler(
    ITaskRepository taskRepository,
    IProjectRepository projectRepository)
    : IRequestHandler<GetTasksQuery, IEnumerable<TaskResponse>>
{
    public async Task<IEnumerable<TaskResponse>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        // Verify ownership before returning data
        _ = await projectRepository.GetByIdForUserAsync(request.ProjectId, request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Project), request.ProjectId);

        var tasks = await taskRepository.GetAllByProjectAsync(request.ProjectId, cancellationToken);
        return tasks.Select(TaskQueryMapper.ToResponse);
    }
}

// ════════════════════════════════════════════════════════════
//  GET TASK BY ID
// ════════════════════════════════════════════════════════════

public record GetTaskByIdQuery(Guid TaskId, Guid ProjectId, Guid UserId) : IRequest<TaskResponse>;

public class GetTaskByIdQueryHandler(
    ITaskRepository taskRepository,
    IProjectRepository projectRepository)
    : IRequestHandler<GetTaskByIdQuery, TaskResponse>
{
    public async Task<TaskResponse> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        _ = await projectRepository.GetByIdForUserAsync(request.ProjectId, request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Project), request.ProjectId);

        var task = await taskRepository.GetByIdForProjectAsync(request.TaskId, request.ProjectId, cancellationToken)
            ?? throw new NotFoundException(nameof(ProjectTask), request.TaskId);

        return TaskQueryMapper.ToResponse(task);
    }
}
