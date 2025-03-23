namespace Admin.WebAPI.Endpoints.ProductsVariant.Models;

public record VariantInventoryResponse
{
    public int Stock { get; init; }
    public bool TrackInventory { get; init; }
    public bool AllowBackorders { get; init; }
    public int? LowStockThreshold { get; init; }
    public bool IsLowStock { get; init; }
    public bool IsOutOfStock { get; init; }
}
