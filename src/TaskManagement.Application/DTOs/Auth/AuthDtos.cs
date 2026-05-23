namespace TaskManagement.Application.DTOs.Auth;

public record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password);

public record LoginRequest(
    string Email,
    string Password);

public record AuthResponse(
    Guid UserId,
    string Email,
    string FullName,
    string Role,
    string Token,
    DateTime ExpiresAt);
