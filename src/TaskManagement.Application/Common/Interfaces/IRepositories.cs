using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Common.Interfaces;


public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Delete(T entity); 
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}


public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
}

public interface IProjectRepository : IRepository<Project>
{
    Task<Project?> GetByIdWithTasksAsync(Guid projectId, Guid userId, CancellationToken ct = default);
    Task<IEnumerable<Project>> GetAllByUserAsync(Guid userId, CancellationToken ct = default);
    Task<Project?> GetByIdForUserAsync(Guid projectId, Guid userId, CancellationToken ct = default);
}

public interface ITaskRepository : IRepository<ProjectTask>
{
    Task<ProjectTask?> GetByIdForProjectAsync(Guid taskId, Guid projectId, CancellationToken ct = default);
    Task<IEnumerable<ProjectTask>> GetAllByProjectAsync(Guid projectId, CancellationToken ct = default);
}


public interface IJwtService
{
    string GenerateToken(User user);
}


public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}


public interface ICurrentUserService
{
    Guid UserId { get; }
    string Email { get; }
    string Role { get; }
    bool IsAuthenticated { get; }
}
