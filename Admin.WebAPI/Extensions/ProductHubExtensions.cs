using Admin.WebAPI.Hubs.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace Admin.WebAPI.Extensions;

public static class ProductHubExtensions
{
    public static HubConnection AddProductInventoryHandlers(
        this HubConnection connection,
        Action<ProductInventoryUpdate> onInventoryUpdate)
    {
        connection.On<ProductInventoryUpdate>(
            "InventoryUpdated",
            update => onInventoryUpdate(update));

        return connection;
    }

    public static HubConnection AddProductPriceHandlers(
        this HubConnection connection,
        Action<ProductPriceUpdate> onPriceUpdate)
    {
        connection.On<ProductPriceUpdate>(
            "PriceUpdated",
            update => onPriceUpdate(update));

        return connection;
    }

    public static HubConnection AddProductStatusHandlers(
        this HubConnection connection,
        Action<ProductStatusUpdate> onStatusUpdate)
    {
        connection.On<ProductStatusUpdate>(
            "StatusUpdated",
            update => onStatusUpdate(update));

        return connection;
    }
}

