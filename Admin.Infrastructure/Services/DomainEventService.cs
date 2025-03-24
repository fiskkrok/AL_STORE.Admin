using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using Admin.Domain.Common;
using MediatR;

namespace Admin.Infrastructure.Services;
public class DomainEventService : ServiceBase, IDomainEventService
{
    private readonly IPublisher _mediator;
    private readonly IMessageBusService _messageBus;
    private readonly IDomainEventMapper _eventMapper;

    public DomainEventService(
        IPublisher mediator,
        IMessageBusService messageBus,
        IDomainEventMapper eventMapper)
    {
        _mediator = mediator;
        _messageBus = messageBus;
        _eventMapper = eventMapper;
    }

    public async Task PublishAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await ExecuteWithErrorHandlingAsync(async () =>
            {
                // Create notification for in-memory handlers
                var notification = CreateDomainEventNotification(domainEvent);
                await _mediator.Publish(notification, cancellationToken);

                // Map to integration event if needed and publish to message bus
                var integrationEvent = _eventMapper.MapToIntegrationEvent(domainEvent);
                if (integrationEvent != null)
                {
                    await _messageBus.PublishAsync(integrationEvent, cancellationToken);
                }

                return Task.CompletedTask;
            }, "DomainEvent.PublishFailed", $"Failed to publish domain event {domainEvent.GetType().Name}");
    }

    private static INotification CreateDomainEventNotification(DomainEvent domainEvent)
    {
        var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
        return (INotification)Activator.CreateInstance(notificationType, domainEvent)!;
    }
}