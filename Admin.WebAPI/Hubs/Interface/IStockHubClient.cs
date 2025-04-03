// Admin.WebAPI/Hubs/Interface/IStockHubClient.cs
using Admin.Application.Inventory.DTOs;

namespace Admin.WebAPI.Hubs.Interface;

public interface IStockHubClient
{
    Task StockUpdated(StockItemDto stockItem);
    Task LowStockAlert(StockItemDto stockItem);
    Task OutOfStockAlert(StockItemDto stockItem);
    Task InventoryNotification(object notification);
}