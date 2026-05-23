namespace TaskManagement.Domain.Common;

/// <summary>
/// Base entity with audit fields and soft-delete support.
/// All entities inherit this to get consistent tracking columns.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Soft delete — records are never physically deleted
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
}
