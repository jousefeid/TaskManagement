using System.Net;
using System.Text.Json;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Models;

namespace TaskManagement.API.Middleware;


public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            Application.Common.Exceptions.ValidationException ve =>
                (HttpStatusCode.BadRequest, ApiResponse.Fail(
                    "Validation failed.",
                    ve.Errors.SelectMany(e => e.Value))),

            NotFoundException nfe =>
                (HttpStatusCode.NotFound, ApiResponse.Fail(nfe.Message)),

            UnauthorizedException ue =>
                (HttpStatusCode.Unauthorized, ApiResponse.Fail(ue.Message)),

            ForbiddenException fe =>
                (HttpStatusCode.Forbidden, ApiResponse.Fail(fe.Message)),

            ConflictException ce =>
                (HttpStatusCode.Conflict, ApiResponse.Fail(ce.Message)),

            _ => (HttpStatusCode.InternalServerError,
                  ApiResponse.Fail("An unexpected error occurred. Please try again later."))
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}
