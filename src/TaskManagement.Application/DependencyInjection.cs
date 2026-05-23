using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TaskManagement.Application.Common.Behaviors;

namespace TaskManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // Register all MediatR handlers from this assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // Register all FluentValidation validators from this assembly
        services.AddValidatorsFromAssembly(assembly);

        // Register the validation pipeline behavior
        // This runs validators automatically before every MediatR handler
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
