using TaskManagement.Domain.Common;
using TaskManagement.Domain.Enums;
using TaskStatusEnum = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Domain.Entities;

public class ProjectTask : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskStatusEnum Status { get; set; } = TaskStatusEnum.Todo;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime? DueDate { get; set; }

    // Foreign key — tasks belong to a project
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
}
