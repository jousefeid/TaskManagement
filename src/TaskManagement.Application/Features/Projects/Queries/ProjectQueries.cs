using MediatR;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.DTOs.Projects;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Features.Projects.Queries;

// Shared mapper for query handlers
internal static class ProjectQueryMapper
{
    public static ProjectResponse ToResponse(Project project) => new(
        project.Id,
        project.Name,
        project.Description,
        project.CreatedAt,
        project.Tasks.Count(t => !t.IsDeleted));
}

// ════════════════════════════════════════════════════════════
//  GET ALL PROJECTS for current user
// ════════════════════════════════════════════════════════════

public record GetProjectsQuery(Guid UserId) : IRequest<IEnumerable<ProjectResponse>>;

public class GetProjectsQueryHandler(IProjectRepository projectRepository)
    : IRequestHandler<GetProjectsQuery, IEnumerable<ProjectResponse>>
{
    public async Task<IEnumerable<ProjectResponse>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        var projects = await projectRepository.GetAllByUserAsync(request.UserId, cancellationToken);
        return projects.Select(ProjectQueryMapper.ToResponse);
    }
}

// ════════════════════════════════════════════════════════════
//  GET PROJECT BY ID (with tasks)
// ════════════════════════════════════════════════════════════

public record GetProjectByIdQuery(Guid ProjectId, Guid UserId) : IRequest<ProjectResponse>;

public class GetProjectByIdQueryHandler(IProjectRepository projectRepository)
    : IRequestHandler<GetProjectByIdQuery, ProjectResponse>
{
    public async Task<ProjectResponse> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var project = await projectRepository.GetByIdWithTasksAsync(request.ProjectId, request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Project), request.ProjectId);

        return ProjectQueryMapper.ToResponse(project);
    }
}
