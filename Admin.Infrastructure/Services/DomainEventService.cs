using Admin.Application.Common.Exceptions;
using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using Admin.Domain.Common;

using MediatR;
using Microsoft.Extensions.Logging;

namespace Admin.Infrastructure.Services;
public class DomainEventService : ServiceBase, IDomainEventService
{
    private readonly IPublisher _mediator;
    private readonly IMessageBusService _messageBus;
    private readonly IDomainEventMapper _eventMapper;
    private readonly ILogger<DomainEventService> _logger;

    public DomainEventService(
        IPublisher mediator,
        IMessageBusService messageBus,
        IDomainEventMapper eventMapper, ILogger<DomainEventService> logger)
    {
        _mediator = mediator;
        _messageBus = messageBus;
        _eventMapper = eventMapper;
        _logger = logger;
    }
    public async Task PublishAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            // Create notification for in-memory handlers
            var notification = CreateDomainEventNotification(domainEvent);
            await _mediator.Publish(notification, cancellationToken);

            // Map to integration event if needed and publish to message bus
            var integrationEvent = _eventMapper.MapToIntegrationEvent(domainEvent);
            if (integrationEvent != null)
            {
                try
                {
                    await _messageBus.PublishAsync(integrationEvent, cancellationToken);
                }
                catch (Exception ex)
                {
                    // Log but don't crash the app if message bus is unavailable
                    _logger.LogError(ex, "Failed to publish integration event {EventType} to message bus",
                        integrationEvent.GetType().Name);

                    // You could add code here to store failed messages for later retry
                    // For example, in a database table or in-memory queue
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing domain event {EventType}", domainEvent.GetType().Name);
            throw new AppException("DomainEvent.PublishFailed",
                $"Failed to publish domain event {domainEvent.GetType().Name}", ex);
        }
    }
    //public async Task PublishAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default)
    //{
    //    await ExecuteWithErrorHandlingAsync(async () =>
    //        {
    //            // Create notification for in-memory handlers
    //            var notification = CreateDomainEventNotification(domainEvent);
    //            await _mediator.Publish(notification, cancellationToken);

    //            // Map to integration event if needed and publish to message bus
    //            var integrationEvent = _eventMapper.MapToIntegrationEvent(domainEvent);
    //            if (integrationEvent != null)
    //            {
    //                await _messageBus.PublishAsync(integrationEvent, cancellationToken);
    //            }

    //            return Task.CompletedTask;
    //        }, "DomainEvent.PublishFailed", $"Failed to publish domain event {domainEvent.GetType().Name}");
    //}

    private static INotification CreateDomainEventNotification(DomainEvent domainEvent)
    {
        var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
        return (INotification)Activator.CreateInstance(notificationType, domainEvent)!;
    }
}