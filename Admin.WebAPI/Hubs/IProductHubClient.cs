using Admin.Application.Products.DTOs;

namespace Admin.WebAPI.Hubs;

public interface IProductHubClient
{
    Task ProductCreated(ProductDto product);
    Task ProductUpdated(ProductDto product);
    Task ProductDeleted(Guid productId);
    Task StockUpdated(Guid productId, int newStock);
    Task ImageProcessed(Guid productId, Guid imageId, string processedUrl);
}