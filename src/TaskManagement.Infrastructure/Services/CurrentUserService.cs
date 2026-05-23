using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TaskManagement.Application.Common.Interfaces;

namespace TaskManagement.Infrastructure.Services;

/// <summary>
/// Extracts the current user's identity from the HTTP request's JWT claims.
/// Registered as Scoped so it's re-created per request with fresh HttpContext.
/// </summary>
public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid UserId
    {
        get
        {
            var value = User?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User?.FindFirstValue("sub");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }

    public string Email => User?.FindFirstValue(ClaimTypes.Email)
        ?? User?.FindFirstValue("email")
        ?? string.Empty;

    public string Role => User?.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
}
