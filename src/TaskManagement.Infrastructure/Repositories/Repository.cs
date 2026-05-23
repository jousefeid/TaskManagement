using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Common;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Repositories;

/// <summary>
/// Generic repository implementation. Soft delete is handled here centrally —
/// calling Delete() sets IsDeleted and DeletedAt rather than removing the row.
/// The EF global query filters in AppDbContext ensure deleted records are invisible
/// to all subsequent queries unless explicitly bypassed with IgnoreQueryFilters().
/// </summary>
public class Repository<T>(AppDbContext context) : IRepository<T> where T : BaseEntity
{
    protected readonly DbSet<T> DbSet = context.Set<T>();

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await DbSet.FindAsync([id], ct);

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default) =>
        await DbSet.ToListAsync(ct);

    public virtual async Task AddAsync(T entity, CancellationToken ct = default) =>
        await DbSet.AddAsync(entity, ct);

    public virtual void Update(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        DbSet.Update(entity);
    }

    public virtual void Delete(T entity)
    {
        // Soft delete: flag the record instead of removing it
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        DbSet.Update(entity);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await context.SaveChangesAsync(ct);
}
