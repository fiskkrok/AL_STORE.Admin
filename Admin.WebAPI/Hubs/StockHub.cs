// Admin.WebAPI/Hubs/StockHub.cs
using Admin.WebAPI.Hubs.Interface;
using Admin.WebAPI.Infrastructure.Authorization;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Admin.WebAPI.Hubs;

[Authorize(AuthenticationSchemes = $"{AuthConstants.JwtBearerScheme},{AuthConstants.ApiKeyScheme}")]
public class StockHub : Hub<IStockHubClient>
{
    private readonly ILogger<StockHub> _logger;

    public StockHub(ILogger<StockHub> logger)
    {
        _logger = logger;
    }
    
    public override async Task OnConnectedAsync()
    {
        var user = Context.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            // Add users to appropriate groups based on their roles
            var isAdmin = user.IsInRole(AuthConstants.SystemAdministratorRole) ||
                                            user.HasClaim(c => c is { Type: "scope", Value: AuthConstants.FullApiAccess });
            var isInventoryManager = user.IsInRole(AuthConstants.InventoryManagerRole) ||
                                         user.HasClaim(c => c is { Type: "Permission", Value: "Inventory.Manage" });

            if (isAdmin || isInventoryManager)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "InventoryManagers");
                _logger.LogInformation("Inventory manager connected to StockHub: {ConnectionId}", Context.ConnectionId);
            }
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "InventoryManagers");
        _logger.LogInformation("User disconnected from StockHub: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SubscribeToProductStock(string productId)
    {
        var groupName = $"stock-product-{productId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("User {ConnectionId} subscribed to stock for product {ProductId}",
            Context.ConnectionId, productId);
    }

    public async Task UnsubscribeFromProductStock(string productId)
    {
        var groupName = $"stock-product-{productId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("User {ConnectionId} unsubscribed from stock for product {ProductId}",
            Context.ConnectionId, productId);
    }

    public async Task SubscribeToLowStockAlerts()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "LowStockAlerts");
        _logger.LogInformation("User {ConnectionId} subscribed to low stock alerts", Context.ConnectionId);
    }

    public async Task UnsubscribeFromLowStockAlerts()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "LowStockAlerts");
        _logger.LogInformation("User {ConnectionId} unsubscribed from low stock alerts", Context.ConnectionId);
    }
}