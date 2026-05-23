using FluentValidation;
using MediatR;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.DTOs.Projects;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Features.Projects.Commands;

// ════════════════════════════════════════════════════════════
//  SHARED MAPPING — Manual mapping, no AutoMapper
// ════════════════════════════════════════════════════════════

internal static class ProjectMapper
{
    public static ProjectResponse ToResponse(Project project) => new(
        project.Id,
        project.Name,
        project.Description,
        project.CreatedAt,
        project.Tasks.Count(t => !t.IsDeleted));
}

// ════════════════════════════════════════════════════════════
//  CREATE PROJECT
// ════════════════════════════════════════════════════════════

public record CreateProjectCommand(string Name, string Description, Guid UserId) : IRequest<ProjectResponse>;

public class CreateProjectCommandHandler(IProjectRepository projectRepository)
    : IRequestHandler<CreateProjectCommand, ProjectResponse>
{
    public async Task<ProjectResponse> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = new Project
        {
            Name = request.Name,
            Description = request.Description,
            UserId = request.UserId,
            Tasks = []
        };

        await projectRepository.AddAsync(project, cancellationToken);
        await projectRepository.SaveChangesAsync(cancellationToken);

        return ProjectMapper.ToResponse(project);
    }
}

public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.UserId).NotEmpty();
    }
}

// ════════════════════════════════════════════════════════════
//  UPDATE PROJECT
// ════════════════════════════════════════════════════════════

public record UpdateProjectCommand(Guid ProjectId, string Name, string Description, Guid UserId) : IRequest<ProjectResponse>;

public class UpdateProjectCommandHandler(IProjectRepository projectRepository)
    : IRequestHandler<UpdateProjectCommand, ProjectResponse>
{
    public async Task<ProjectResponse> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await projectRepository.GetByIdWithTasksAsync(request.ProjectId, request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Project), request.ProjectId);

        project.Name = request.Name;
        project.Description = request.Description;
        project.UpdatedAt = DateTime.UtcNow;

        projectRepository.Update(project);
        await projectRepository.SaveChangesAsync(cancellationToken);

        return ProjectMapper.ToResponse(project);
    }
}

public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}

// ════════════════════════════════════════════════════════════
//  DELETE PROJECT (soft delete)
// ════════════════════════════════════════════════════════════

public record DeleteProjectCommand(Guid ProjectId, Guid UserId) : IRequest;

public class DeleteProjectCommandHandler(IProjectRepository projectRepository)
    : IRequestHandler<DeleteProjectCommand>
{
    public async Task Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await projectRepository.GetByIdForUserAsync(request.ProjectId, request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Project), request.ProjectId);

        projectRepository.Delete(project); // soft delete: sets IsDeleted = true
        await projectRepository.SaveChangesAsync(cancellationToken);
    }
}
