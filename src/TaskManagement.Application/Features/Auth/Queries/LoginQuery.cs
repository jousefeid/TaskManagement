using FluentValidation;
using MediatR;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.DTOs.Auth;

namespace TaskManagement.Application.Features.Auth.Queries;

// ── Query ────────────────────────────────────────────────────────────────────

public record LoginQuery(string Email, string Password) : IRequest<AuthResponse>;

// ── Handler ──────────────────────────────────────────────────────────────────

public class LoginQueryHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtService jwtService) : IRequestHandler<LoginQuery, AuthResponse>
{
    public async Task<AuthResponse> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email.ToLowerInvariant(), cancellationToken)
            ?? throw new UnauthorizedException("Invalid email or password.");

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password.");

        var token = jwtService.GenerateToken(user);

        return new AuthResponse(
            user.Id,
            user.Email,
            user.FullName,
            user.Role.ToString(),
            token,
            DateTime.UtcNow.AddHours(24));
    }
}

// ── Validator ─────────────────────────────────────────────────────────────────

public class LoginQueryValidator : AbstractValidator<LoginQuery>
{
    public LoginQueryValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
