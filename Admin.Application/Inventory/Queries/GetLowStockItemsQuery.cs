using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using Admin.Application.Inventory.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Admin.Application.Inventory.Queries;
public record GetLowStockItemsQuery : IRequest<Result<List<StockItemDto>>>;

public class GetLowStockItemsQueryHandler : IRequestHandler<GetLowStockItemsQuery, Result<List<StockItemDto>>>
{
    
    private readonly IProductRepository _productRepository;
    private readonly IStockRepository _stockRepository;
    private readonly ILogger<GetLowStockItemsQueryHandler> _logger;

    public GetLowStockItemsQueryHandler(
        IStockRepository stockRepository,
        IProductRepository productRepository,
        ILogger<GetLowStockItemsQueryHandler> logger)
    {
        _stockRepository = stockRepository;
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<Result<List<StockItemDto>>> Handle(GetLowStockItemsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _stockRepository.GetLowStockItemsAsync(cancellationToken);

            // Get all product IDs to fetch product names in a single query
            var productIds = items.Select(i => i.ProductId).ToList();
            var products = await _productRepository.GetProductsByIdsAsync(productIds, cancellationToken);

            // Create a lookup dictionary for quick product name retrieval
            var productNameLookup = products.ToDictionary(p => p.Id, p => p.Name);

            var dtos = items.Select(stockItem => new StockItemDto
            {
                Id = stockItem.Id,
                ProductId = stockItem.ProductId,
                ProductName = productNameLookup.TryGetValue(stockItem.ProductId, out var name) ? name : "Unknown Product",
                CurrentStock = stockItem.CurrentStock,
                ReservedStock = stockItem.ReservedStock,
                AvailableStock = stockItem.AvailableStock,
                LowStockThreshold = stockItem.LowStockThreshold,
                TrackInventory = stockItem.TrackInventory,
                IsLowStock = stockItem.IsLowStock,
                IsOutOfStock = stockItem.IsOutOfStock,
                Reservations = stockItem.Reservations.Select(r => new StockReservationDto
                {
                    Id = r.Id,
                    OrderId = r.OrderId,
                    Quantity = r.Quantity,
                    Status = r.Status.ToString(),
                    ExpiresAt = r.ExpiresAt,
                    ConfirmedAt = r.ConfirmedAt,
                    CancelledAt = r.CancelledAt
                }).ToList()
            }).ToList();

            return Result<List<StockItemDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving low stock items");
            return Result<List<StockItemDto>>.Failure(new Error("StockItem.GetLowStockFailed", "Failed to retrieve low stock items"));
        }
    }
}
