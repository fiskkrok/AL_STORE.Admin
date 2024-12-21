using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using Admin.Application.Inventory.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Admin.Application.Inventory.Queries;
public record GetStockHistoryQuery : IRequest<Result<List<StockHistoryDto>>>
{
    public Guid ProductId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}

public record StockHistoryDto
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public int OldStock { get; init; }
    public int NewStock { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string? ModifiedBy { get; init; }
    public DateTime ModifiedAt { get; init; }
}

public record GetOutOfStockItemsQuery : IRequest<Result<List<StockItemDto>>>;

public class GetOutOfStockItemsQueryHandler : IRequestHandler<GetOutOfStockItemsQuery, Result<List<StockItemDto>>>
{
    private readonly IStockRepository _stockRepository;
    private readonly ILogger<GetOutOfStockItemsQueryHandler> _logger;

    public GetOutOfStockItemsQueryHandler(
        IStockRepository stockRepository,
        ILogger<GetOutOfStockItemsQueryHandler> logger)
    {
        _stockRepository = stockRepository;
        _logger = logger;
    }

    public async Task<Result<List<StockItemDto>>> Handle(GetOutOfStockItemsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _stockRepository.GetOutOfStockItemsAsync(cancellationToken);
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
            _logger.LogError(ex, "Error retrieving out of stock items");
            return Result<List<StockItemDto>>.Failure(new Error("StockItem.GetOutOfStockFailed", "Failed to retrieve out of stock items"));
        }
    }
}
