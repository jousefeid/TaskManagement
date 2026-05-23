using TaskManagement.Domain.Common;

namespace TaskManagement.Domain.Entities;

public class Project : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Foreign key — projects are owned by a user
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    // Navigation property
    public ICollection<ProjectTask> Tasks { get; set; } = [];
}
