using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using Admin.Application.Inventory.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Admin.Application.Inventory.Queries;
public record GetStockItemQuery(Guid ProductId) : IRequest<Result<StockItemDto>>;

public class GetStockItemQueryHandler : IRequestHandler<GetStockItemQuery, Result<StockItemDto>>
{
    private readonly IStockRepository _stockRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<GetStockItemQueryHandler> _logger;

    public GetStockItemQueryHandler(
        IStockRepository stockRepository,
        IProductRepository productRepository,
        ILogger<GetStockItemQueryHandler> logger)
    {
        _stockRepository = stockRepository;
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<Result<StockItemDto>> Handle(GetStockItemQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var stockItem = await _stockRepository.GetByProductIdAsync(request.ProductId, cancellationToken);
            if (stockItem == null)
                return Result<StockItemDto>.Failure(new Error("StockItem.NotFound", "Stock item not found"));

            // Get product name for frontend display
            var product = await _productRepository.GetByIdAsync(stockItem.ProductId, cancellationToken);
            string productName = product?.Name ?? "Unknown Product";

            var dto = new StockItemDto
            {
                Id = stockItem.Id,
                ProductId = stockItem.ProductId,
                ProductName = productName,
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
            };

            return Result<StockItemDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stock item for product {ProductId}", request.ProductId);
            return Result<StockItemDto>.Failure(new Error("StockItem.GetFailed", "Failed to retrieve stock item"));
        }
    }
}
