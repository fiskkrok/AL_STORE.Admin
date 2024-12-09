using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using Admin.Application.Products.Events;
using Admin.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Admin.Application.EventHandler;
public class ProductVariantEventHandler :
    INotificationHandler<DomainEventNotification<ProductVariantCreatedDomainEvent>>,
    INotificationHandler<DomainEventNotification<ProductVariantUpdatedDomainEvent>>,
    INotificationHandler<DomainEventNotification<ProductVariantStockUpdatedDomainEvent>>
{
    private readonly IMessageBusService _messageBus;
    private readonly ILogger<ProductVariantEventHandler> _logger;

    public ProductVariantEventHandler(
        IMessageBusService messageBus,
        ILogger<ProductVariantEventHandler> logger)
    {
        _messageBus = messageBus;
        _logger = logger;
    }

    public async Task Handle(
        DomainEventNotification<ProductVariantCreatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        try
        {
            await _messageBus.PublishAsync(new ProductVariantCreatedIntegrationEvent
            {
                ProductId = domainEvent.Product.Id,
                VariantId = domainEvent.Variant.Id,
                Sku = domainEvent.Variant.Sku,
                Price = domainEvent.Variant.Price.Amount,
                Currency = domainEvent.Variant.Price.Currency,
                Stock = domainEvent.Variant.Stock
            }, cancellationToken);

            _logger.LogInformation(
                "Published ProductVariantCreatedIntegrationEvent for product {ProductId}, variant {VariantId}",
                domainEvent.Product.Id,
                domainEvent.Variant.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error publishing ProductVariantCreatedIntegrationEvent for product {ProductId}, variant {VariantId}",
                domainEvent.Product.Id,
                domainEvent.Variant.Id);
            throw;
        }
    }

    public async Task Handle(
        DomainEventNotification<ProductVariantUpdatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        // Similar to created event handling
    }

    public async Task Handle(
        DomainEventNotification<ProductVariantStockUpdatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        try
        {
            await _messageBus.PublishAsync(new ProductVariantStockUpdatedIntegrationEvent
            {
                ProductId = domainEvent.Product.Id,
                VariantId = domainEvent.Variant.Id,
                NewStock = domainEvent.NewStock
            }, cancellationToken);

            _logger.LogInformation(
                "Published ProductVariantStockUpdatedIntegrationEvent for product {ProductId}, variant {VariantId}",
                domainEvent.Product.Id,
                domainEvent.Variant.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error publishing ProductVariantStockUpdatedIntegrationEvent for product {ProductId}, variant {VariantId}",
                domainEvent.Product.Id,
                domainEvent.Variant.Id);
            throw;
        }
    }
}
