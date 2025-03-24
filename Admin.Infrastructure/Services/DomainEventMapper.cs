using Admin.Application.Common.Interfaces;
using Admin.Domain.Common;
using Admin.Domain.Events.Product;
using Admin.Application.Products.Events;

namespace Admin.Infrastructure.Services;

public class DomainEventMapper : IDomainEventMapper
{
    public IMessage? MapToIntegrationEvent(DomainEvent domainEvent)
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