namespace Admin.WebAPI.Hubs.Models;

public record VariantPriceUpdate
{
    public Guid ProductId { get; init; }
    public Guid VariantId { get; init; }
    public decimal NewPrice { get; init; }
    public decimal? OldPrice { get; init; }
    public string Currency { get; init; } = string.Empty;
    public decimal? CompareAtPrice { get; init; }
    public decimal? CostPrice { get; init; }
    public string UpdatedBy { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}