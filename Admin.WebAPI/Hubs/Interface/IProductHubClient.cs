using Admin.Application.Products.DTOs;
using Admin.WebAPI.Hubs.Models;

namespace Admin.WebAPI.Hubs.Interface;

public interface IProductHubClient
{
    Task ProductCreated(ProductDto product);
    Task ProductUpdated(ProductDto product);
    Task ProductDeleted(Guid productId);
    Task StockUpdated(Guid productId, int newStock);
    Task ImageProcessed(Guid productId, Guid imageId, string processedUrl);
    Task VariantCreated(Guid productId, ProductVariantDto variant);
    Task VariantUpdated(Guid productId, ProductVariantDto variant);
    Task VariantDeleted(Guid productId, Guid variantId);
    Task VariantStockUpdated(Guid productId, Guid variantId, int newStock);
    Task UserConnected(object connectionInfo);
    Task UserDisconnected(object disconnectionInfo);
    Task InventoryAlert(object alert);
    Task InventoryUpdated(ProductInventoryUpdate update);
    Task PriceUpdated(ProductPriceUpdate update);
    Task StatusUpdated(ProductStatusUpdate update);
}