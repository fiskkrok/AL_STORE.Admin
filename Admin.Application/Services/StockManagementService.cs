// Admin.Application/Services/StockManagementService.cs
using Admin.Application.Common.Interfaces;
using Admin.Domain.Entities;

using Microsoft.Extensions.Logging;

namespace Admin.Application.Services;

public class StockManagementService
{
    private readonly IProductRepository _productRepo;
    private readonly IStockRepository _stockRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<StockManagementService> _logger;

    public StockManagementService(
        IUnitOfWork unitOfWork,
        ILogger<StockManagementService> logger, IStockRepository stockRepo, IProductRepository productRepo)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _stockRepo = stockRepo;
        _productRepo = productRepo;
    }

    public async Task<StockItem> GetOrCreateStockItemAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        // First, try to get existing stock item
        var stockItem = await _stockRepo.GetByProductIdAsync(productId, cancellationToken);

        // If it exists, just return it
        if (stockItem != null)
            return stockItem;

        // Otherwise, create a default stock item with conservative defaults
        _logger.LogInformation("Creating default stock item for product {ProductId}", productId);

        var defaultStockItem = new StockItem(
            productId: productId,
            initialStock: 0,           // Start with 0 stock by default
            lowStockThreshold: 5,      // Default low stock threshold
            trackInventory: true);     // Enable inventory tracking by default

        try
        {
            await _stockRepo.AddAsync(defaultStockItem, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully created default stock item for product {ProductId}", productId);
            return defaultStockItem;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create default stock item for product {ProductId}", productId);
            throw;
        }
    }
    public async Task SyncProductWithStockItem(Guid productId, CancellationToken cancellationToken = default)
    {
        var product = await _productRepo.GetByIdAsync(productId, cancellationToken);
        if (product == null) return;

        var stockItem = await _stockRepo.GetByProductIdAsync(productId, cancellationToken);
        if (stockItem == null)
        {
            // Create a new stock item based on Product.Stock
            stockItem = new StockItem(
                productId: productId,
                initialStock: product.Stock, // Use existing value
                lowStockThreshold: 5,
                trackInventory: true);

            await _stockRepo.AddAsync(stockItem, cancellationToken);
        }
        else
        {
            // Sync product stock from stock item
            product.SyncStockFromStockItem(stockItem);
            await _productRepo.UpdateAsync(product, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task SyncAllProducts(CancellationToken cancellationToken = default)
    {
        // Implementation to sync all products...
    }
}