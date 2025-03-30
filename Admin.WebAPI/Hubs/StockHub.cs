using Microsoft.AspNetCore.SignalR;

namespace Admin.WebAPI.Hubs;

public class StockHub : Hub
{
    public async Task SendStockUpdate(Guid productId, int newStock)
    {
        await Clients.All.SendAsync("StockUpdated", new { ProductId = productId, NewStock = newStock });
    }

    public async Task SendVariantStockUpdate(Guid productId, Guid variantId, int newStock)
    {
        await Clients.All.SendAsync("VariantStockUpdated", new { ProductId = productId, VariantId = variantId, NewStock = newStock });
    }
}