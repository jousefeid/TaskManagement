using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Repositories;

// ── User Repository ──────────────────────────────────────────────────────────

public class UserRepository(AppDbContext context)
    : Repository<User>(context), IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        await DbSet.FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default) =>
        await DbSet.AnyAsync(u => u.Email == email, ct);
}

// ── Project Repository ───────────────────────────────────────────────────────

public class ProjectRepository(AppDbContext context)
    : Repository<Project>(context), IProjectRepository
{
    public async Task<Project?> GetByIdWithTasksAsync(Guid projectId, Guid userId, CancellationToken ct = default) =>
        await DbSet
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId, ct);

    public async Task<IEnumerable<Project>> GetAllByUserAsync(Guid userId, CancellationToken ct = default) =>
        await DbSet
            .Include(p => p.Tasks)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

    public async Task<Project?> GetByIdForUserAsync(Guid projectId, Guid userId, CancellationToken ct = default) =>
        await DbSet
            .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId, ct);
}

// ── Task Repository ──────────────────────────────────────────────────────────

public class TaskRepository(AppDbContext context)
    : Repository<ProjectTask>(context), ITaskRepository
{
    public async Task<ProjectTask?> GetByIdForProjectAsync(Guid taskId, Guid projectId, CancellationToken ct = default) =>
        await DbSet
            .FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId, ct);

    public async Task<IEnumerable<ProjectTask>> GetAllByProjectAsync(Guid projectId, CancellationToken ct = default) =>
        await DbSet
            .Where(t => t.ProjectId == projectId)
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.DueDate)
            .ToListAsync(ct);
}
