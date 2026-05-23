using FluentValidation;
using MediatR;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.DTOs.Auth;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Features.Auth.Commands;


public record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password) : IRequest<AuthResponse>;

// ── Handler ─────────────────────────────────────────────────────────────────

public class RegisterCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtService jwtService) : IRequestHandler<RegisterCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Check for duplicate email
        if (await userRepository.EmailExistsAsync(request.Email, cancellationToken))
            throw new ConflictException($"Email '{request.Email}' is already registered.");

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = passwordHasher.Hash(request.Password),
            Role = UserRole.User
        };

        await userRepository.AddAsync(user, cancellationToken);
        await userRepository.SaveChangesAsync(cancellationToken);

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


public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.");
    }
}
