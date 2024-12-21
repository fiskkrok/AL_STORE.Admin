using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using Admin.Application.Products.Events;
using Admin.Domain.Common;
using Admin.Domain.Events.Product;

using MediatR;

using Microsoft.Extensions.Logging;

namespace Admin.Infrastructure.Services;
public class DomainEventService : IDomainEventService
{
    private readonly IPublisher _mediator;
    private readonly ILogger<DomainEventService> _logger;
    private readonly IMessageBusService _messageBus;

    public DomainEventService(
        IPublisher mediator,
        ILogger<DomainEventService> logger,
        IMessageBusService messageBus)
    {
        _mediator = mediator;
        _logger = logger;
        _messageBus = messageBus;
    }

    public async Task PublishAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Publishing domain event {EventType} with ID {EventId}",
            domainEvent.GetType().Name,
            domainEvent.EventId);

        // Create notification
        var notification = CreateDomainEventNotification(domainEvent);

        try
        {
            // Publish to in-memory handlers
            await _mediator.Publish(notification, cancellationToken);

            // Map to integration event if needed
            var integrationEvent = MapToIntegrationEvent(domainEvent);
            if (integrationEvent != null)
            {
                await _messageBus.PublishAsync(integrationEvent, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error publishing domain event {EventType} with ID {EventId}",
                domainEvent.GetType().Name,
                domainEvent.EventId);
            throw;
        }
    }

    private static INotification CreateDomainEventNotification(DomainEvent domainEvent)
    {
        var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
        return (INotification)Activator.CreateInstance(notificationType, domainEvent)!;
    }

    private static IMessage? MapToIntegrationEvent(DomainEvent domainEvent)
    {
        // Map domain events to integration events based on type
        return domainEvent switch
        {
            ProductCreatedDomainEvent e => new ProductCreatedIntegrationEvent
            {
                ProductId = e.Product.Id,
                Name = e.Product.Name,
                Price = e.Product.Price.Amount,
                Currency = e.Product.Price.Currency,
                Stock = e.Product.Stock
            },
            ProductUpdatedDomainEvent e => new ProductUpdatedIntegrationEvent
            {
                ProductId = e.Product.Id,
                Name = e.Product.Name,
                Price = e.Product.Price.Amount,
                Currency = e.Product.Price.Currency
            },
            ProductStockUpdatedDomainEvent e => new ProductStockUpdatedIntegrationEvent
            {
                ProductId = e.Product.Id,
                NewStock = e.NewStock,
            },
            ProductDeletedDomainEvent e => new ProductDeletedIntegrationEvent
            {
                ProductId = e.Product.Id
            },
            ProductImageProcessingEvent e => new ImageProcessedIntegrationEvent
            {
                ProductId = e.ProductId,
                ImageId = e.ImageId,
                ProcessedUrl = e.ProcessedUrl
            },
            _ => null
        };
    }
}