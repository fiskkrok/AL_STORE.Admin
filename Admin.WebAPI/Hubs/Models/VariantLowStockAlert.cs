namespace Admin.WebAPI.Hubs.Models;

public record VariantLowStockAlert
{
    public Guid ProductId { get; init; }
    public Guid VariantId { get; init; }
    public int CurrentStock { get; init; }
    public int LowStockThreshold { get; init; }
    public DateTime Timestamp { get; init; }
}