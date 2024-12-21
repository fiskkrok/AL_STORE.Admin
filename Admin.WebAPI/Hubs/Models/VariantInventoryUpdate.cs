namespace Admin.WebAPI.Hubs.Models;

public record VariantInventoryUpdate
{
    public Guid ProductId { get; init; }
    public Guid VariantId { get; init; }
    public int NewStock { get; init; }
    public bool TrackInventory { get; init; }
    public bool AllowBackorders { get; init; }
    public int? LowStockThreshold { get; init; }
    public bool IsLowStock { get; init; }
    public string UpdatedBy { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}