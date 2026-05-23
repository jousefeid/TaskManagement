using FluentValidation;
using MediatR;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.DTOs.Tasks;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Features.Tasks.Commands;

// ════════════════════════════════════════════════════════════
//  SHARED MAPPER
// ════════════════════════════════════════════════════════════

internal static class TaskMapper
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
//  CREATE TASK
// ════════════════════════════════════════════════════════════

public record CreateTaskCommand(
    Guid ProjectId,
    Guid UserId,
    string Title,
    string Description,
    TaskPriority Priority,
    DateTime? DueDate) : IRequest<TaskResponse>;

public class CreateTaskCommandHandler(
    ITaskRepository taskRepository,
    IProjectRepository projectRepository)
    : IRequestHandler<CreateTaskCommand, TaskResponse>
{
    public async Task<TaskResponse> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        // Verify the project belongs to this user
        var project = await projectRepository.GetByIdForUserAsync(request.ProjectId, request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Project), request.ProjectId);

        var task = new ProjectTask
        {
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            DueDate = request.DueDate,
            ProjectId = project.Id,
            Status = TaskStatus.Todo
        };

        await taskRepository.AddAsync(task, cancellationToken);
        await taskRepository.SaveChangesAsync(cancellationToken);

        return TaskMapper.ToResponse(task);
    }
}

public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow)
            .When(x => x.DueDate.HasValue)
            .WithMessage("Due date must be in the future.");
    }
}

// ════════════════════════════════════════════════════════════
//  UPDATE TASK
// ════════════════════════════════════════════════════════════

public record UpdateTaskCommand(
    Guid TaskId,
    Guid ProjectId,
    Guid UserId,
    string Title,
    string Description,
    TaskStatus Status,
    TaskPriority Priority,
    DateTime? DueDate) : IRequest<TaskResponse>;

public class UpdateTaskCommandHandler(
    ITaskRepository taskRepository,
    IProjectRepository projectRepository)
    : IRequestHandler<UpdateTaskCommand, TaskResponse>
{
    public async Task<TaskResponse> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        // Confirm the user owns the project that contains this task
        _ = await projectRepository.GetByIdForUserAsync(request.ProjectId, request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Project), request.ProjectId);

        var task = await taskRepository.GetByIdForProjectAsync(request.TaskId, request.ProjectId, cancellationToken)
            ?? throw new NotFoundException(nameof(ProjectTask), request.TaskId);

        task.Title = request.Title;
        task.Description = request.Description;
        task.Status = request.Status;
        task.Priority = request.Priority;
        task.DueDate = request.DueDate;
        task.UpdatedAt = DateTime.UtcNow;

        taskRepository.Update(task);
        await taskRepository.SaveChangesAsync(cancellationToken);

        return TaskMapper.ToResponse(task);
    }
}

public class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Priority).IsInEnum();
    }
}

// ════════════════════════════════════════════════════════════
//  DELETE TASK (soft delete)
// ════════════════════════════════════════════════════════════

public record DeleteTaskCommand(Guid TaskId, Guid ProjectId, Guid UserId) : IRequest;

public class DeleteTaskCommandHandler(
    ITaskRepository taskRepository,
    IProjectRepository projectRepository)
    : IRequestHandler<DeleteTaskCommand>
{
    public async Task Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        _ = await projectRepository.GetByIdForUserAsync(request.ProjectId, request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Project), request.ProjectId);

        var task = await taskRepository.GetByIdForProjectAsync(request.TaskId, request.ProjectId, cancellationToken)
            ?? throw new NotFoundException(nameof(ProjectTask), request.TaskId);

        taskRepository.Delete(task);
        await taskRepository.SaveChangesAsync(cancellationToken);
    }
}
