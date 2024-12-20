using Admin.Application.Common.Events;
using Admin.Application.Common.Interfaces;
using Admin.Infrastructure.Behaviors;
using Admin.Infrastructure.Services;

using MediatR;

using Microsoft.Extensions.DependencyInjection;

namespace Admin.Infrastructure.Configuration;

public static class EventHandlingConfiguration
{
    public static IServiceCollection AddEventHandling(this IServiceCollection services)
    {
        // Register domain event handling behavior
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(DomainEventHandlingBehavior<,>));

        // Register domain event service
        services.AddScoped<IDomainEventService, DomainEventService>();

        // Register all domain event handlers from assembly
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(typeof(IDomainEventHandler<>).Assembly);
        });

        return services;
    }
}