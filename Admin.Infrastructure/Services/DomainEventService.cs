using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using Admin.Application.Products.Events;
using Admin.Domain.Common;
using Admin.Domain.Events;

using MediatR;

using Microsoft.Extensions.Logging;

namespace Admin.Infrastructure.Services;
public class DomainEventService : IDomainEventService
{
    private readonly ILogger<DomainEventService> _logger;
    private readonly IPublisher _mediator;
    private readonly IMessageBusService _messageBus;

    public DomainEventService(
        ILogger<DomainEventService> logger,
        IPublisher mediator,
        IMessageBusService messageBus)
    {
        _logger = logger;
        _mediator = mediator;
        _messageBus = messageBus;
    }

    public async Task PublishAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Publishing domain event {Event}", domainEvent.GetType().Name);

        // First, publish through MediatR for in-process handling
        await _mediator.Publish(GetNotificationCorrespondingToDomainEvent(domainEvent), cancellationToken);

        // Then, publish integration event to message bus if needed
        var integrationEvent = MapToIntegrationEvent(domainEvent);
        if (integrationEvent != null)
        {
            await _messageBus.PublishAsync(integrationEvent, cancellationToken);
        }
    }

    private static INotification GetNotificationCorrespondingToDomainEvent(DomainEvent domainEvent)
    {
        return (INotification)Activator.CreateInstance(
            typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType()),
            domainEvent)!;
    }

    private static IMessage? MapToIntegrationEvent(DomainEvent domainEvent)
    {
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
            ProductStockUpdatedDomainEvent e => new ProductStockUpdatedIntegrationEvent
            {
                ProductId = e.Product.Id,
                NewStock = e.NewStock,
            },
            _ => null
        };
    }

    public async Task Publish(object notification, CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = new CancellationToken()) where TNotification : INotification
    {
        throw new NotImplementedException();
    }
}