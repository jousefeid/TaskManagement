using TaskManagement.Application.Common.Interfaces;

namespace TaskManagement.Infrastructure.Services;

/// <summary>
/// BCrypt password hashing. Work factor of 12 is a good balance between
/// security and performance (takes ~250ms on modern hardware).
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string hash) =>
        BCrypt.Net.BCrypt.Verify(password, hash);
}
