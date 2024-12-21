using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using Admin.Application.Inventory.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Admin.Application.Inventory.Queries;
public record GetLowStockItemsQuery : IRequest<Result<List<StockItemDto>>>;

public class GetLowStockItemsQueryHandler : IRequestHandler<GetLowStockItemsQuery, Result<List<StockItemDto>>>
{
    private readonly IStockRepository _stockRepository;
    private readonly ILogger<GetLowStockItemsQueryHandler> _logger;

    public GetLowStockItemsQueryHandler(
        IStockRepository stockRepository,
        ILogger<GetLowStockItemsQueryHandler> logger)
    {
        _stockRepository = stockRepository;
        _logger = logger;
    }

    public async Task<Result<List<StockItemDto>>> Handle(GetLowStockItemsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _stockRepository.GetLowStockItemsAsync(cancellationToken);
            var dtos = items.Select(stockItem => new StockItemDto
            {
                Id = stockItem.Id,
                ProductId = stockItem.ProductId,
                CurrentStock = stockItem.CurrentStock,
                ReservedStock = stockItem.ReservedStock,
                AvailableStock = stockItem.AvailableStock,
                LowStockThreshold = stockItem.LowStockThreshold,
                TrackInventory = stockItem.TrackInventory,
                IsLowStock = stockItem.IsLowStock,
                IsOutOfStock = stockItem.IsOutOfStock
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
