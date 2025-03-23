namespace Admin.WebAPI.Endpoints.ProductsVariant.Models;

public record VariantInventoryRequest
{
    public int Stock { get; init; }
    public bool TrackInventory { get; init; }
    public bool AllowBackorders { get; init; }
    public int? LowStockThreshold { get; init; }
}
