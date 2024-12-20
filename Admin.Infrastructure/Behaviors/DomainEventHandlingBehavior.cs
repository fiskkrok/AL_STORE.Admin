

using Admin.Application.Common.Interfaces;
using Admin.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Admin.Infrastructure.Behaviors;

public class DomainEventHandlingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly ILogger<DomainEventHandlingBehavior<TRequest, TResponse>> _logger;
    private readonly IDomainEventService _domainEventService;

    public DomainEventHandlingBehavior(
        ILogger<DomainEventHandlingBehavior<TRequest, TResponse>> logger,
        IDomainEventService domainEventService)
    {
        _logger = logger;
        _domainEventService = domainEventService;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Process the request
        var response = await next();

        // If the response contains an entity with domain events, publish them
        if (response is AuditableEntity entity && entity.DomainEvents.Any())
        {
            _logger.LogDebug(
                "Found {EventCount} domain events to process for {EntityType} {EntityId}",
                entity.DomainEvents.Count,
                entity.GetType().Name,
                entity.Id);

            // Process all domain events
            foreach (var domainEvent in entity.DomainEvents.ToList())
            {
                await _domainEventService.PublishAsync(domainEvent, cancellationToken);
            }

            // Clear the domain events
            entity.ClearDomainEvents();
        }

        return response;
    }
}
