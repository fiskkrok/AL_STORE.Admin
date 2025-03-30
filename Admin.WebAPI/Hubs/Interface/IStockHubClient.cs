using Admin.WebAPI.Hubs.Models;

namespace Admin.WebAPI.Hubs.Interface;

public interface IStockHubClient
{
    Task SendStockUpdate(Guid productId, int newStock);
    Task SendLowStockAlert(object alert);
    Task SendInventoryNotification(object notification);
    Task UserDisconnected(object disconnectionInfo);
    Task InventoryAlert(object alert);
}
