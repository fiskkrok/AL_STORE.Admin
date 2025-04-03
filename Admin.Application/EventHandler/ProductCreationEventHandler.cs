// Admin.Application/EventHandler/ProductCreationEventHandler.cs
using Admin.Application.Common.Models;
using Admin.Application.Services;
using Admin.Domain.Events.Product;

using MediatR;

using Microsoft.Extensions.Logging;

namespace Admin.Application.EventHandler;

public class ProductCreationEventHandler : INotificationHandler<DomainEventNotification<ProductCreatedDomainEvent>>
{
    private readonly StockManagementService _stockManagementService;
    private readonly ILogger<ProductCreationEventHandler> _logger;

    public ProductCreationEventHandler(
        StockManagementService stockManagementService,
        ILogger<ProductCreationEventHandler> logger)
    {
        _stockManagementService = stockManagementService;
        _logger = logger;
    }

    public async Task Handle(DomainEventNotification<ProductCreatedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var product = notification.DomainEvent.Product;

        try
        {
            // Automatically create a stock item for the new product
            await _stockManagementService.GetOrCreateStockItemAsync(product.Id, cancellationToken);
            _logger.LogInformation("Auto-created stock item for new product: {ProductId}", product.Id);
        }
        catch (Exception ex)
        {
            // Log but don't rethrow - we don't want product creation to fail if stock creation fails
            _logger.LogError(ex, "Failed to auto-create stock item for new product: {ProductId}", product.Id);
        }
    }
}