using Admin.Application.Inventory.Commands;

namespace Admin.WebAPI.Endpoints.Stock.Models;

public record AdjustStockRequest
{
    public Guid ProductId { get; init; }
    public int Adjustment { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public record BatchAdjustStockRequest
{
    public List<BatchAdjustmentItem> Adjustments { get; init; } = new();
    public string Reason { get; init; } = string.Empty;
}

public record BatchAdjustmentItem
{
    public Guid ProductId { get; init; }
    public int Adjustment { get; init; }
}

public record EmptyResponse;