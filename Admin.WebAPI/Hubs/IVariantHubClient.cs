using Admin.WebAPI.Hubs.Models;

namespace Admin.WebAPI.Hubs;

public interface IVariantHubClient
{
    Task VariantInventoryUpdated(VariantInventoryUpdate update);
    Task VariantPriceUpdated(VariantPriceUpdate update);
    Task VariantLowStock(VariantLowStockAlert alert);
}