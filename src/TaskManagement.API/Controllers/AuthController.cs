using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Common.Models;
using TaskManagement.Application.DTOs.Auth;
using TaskManagement.Application.Features.Auth.Commands;
using TaskManagement.Application.Features.Auth.Queries;

namespace TaskManagement.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var command = new RegisterCommand(request.FirstName, request.LastName, request.Email, request.Password);
        var result = await mediator.Send(command, ct);
        return CreatedAtAction(nameof(Register), ApiResponse<AuthResponse>.Ok(result, "Registration successful."));
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var query = new LoginQuery(request.Email, request.Password);
        var result = await mediator.Send(query, ct);
        return Ok(ApiResponse<AuthResponse>.Ok(result, "Login successful."));
    }
}
