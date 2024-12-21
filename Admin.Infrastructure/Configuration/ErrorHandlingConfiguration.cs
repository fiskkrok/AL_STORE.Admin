using Admin.Application.Common.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Admin.Infrastructure.Configuration;
public static class ErrorHandlingConfiguration
{
    public static IServiceCollection AddErrorHandling(this IServiceCollection services)
    {
        // Register validation behavior for MediatR pipeline
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // Register all validators in the assembly
        services.AddValidatorsFromAssembly(typeof(ValidationBehavior<,>).Assembly);

        return services;
    }
}
