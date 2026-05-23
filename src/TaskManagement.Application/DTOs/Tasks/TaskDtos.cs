using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.DTOs.Tasks;

public record CreateTaskRequest(
    string Title,
    string Description,
    TaskPriority Priority,
    DateTime? DueDate);

public record UpdateTaskRequest(
    string Title,
    string Description,
    TaskStatus Status,
    TaskPriority Priority,
    DateTime? DueDate);

public record TaskResponse(
    Guid Id,
    string Title,
    string Description,
    string Status,
    string Priority,
    DateTime? DueDate,
    Guid ProjectId,
    DateTime CreatedAt);
