using Admin.Application.Common.Events;
using Admin.Application.Common.Models;
using Admin.Domain.Events.Product;

using Microsoft.Extensions.Logging;

namespace Admin.Application.Products.EventHandlers;

public class ProductCreatedEventHandler
    : IDomainEventHandler<ProductCreatedDomainEvent>
{
    private readonly ILogger<ProductCreatedEventHandler> _logger;

    public ProductCreatedEventHandler(ILogger<ProductCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(
        DomainEventNotification<ProductCreatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation(
            "Product {ProductId} was created at {CreatedAt}",
            domainEvent.Product.Id,
            domainEvent.Product.CreatedAt);

        // Handle the event (e.g., send notifications, update indexes, etc.)
        await Task.CompletedTask;
    }
}
