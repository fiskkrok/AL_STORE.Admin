using Admin.WebAPI.Hubs.Interface;
using Admin.WebAPI.Hubs.Models;
using Admin.WebAPI.Infrastructure.Authorization;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace Admin.WebAPI.Hubs;

[Authorize(AuthenticationSchemes = $"{AuthConstants.JwtBearerScheme},{AuthConstants.ApiKeyScheme}")]
public class StockHub : Hub<IStockHubClient>
{
    private readonly ILogger<StockHub> _logger;
    private static readonly ConcurrentDictionary<string, UserConnection> _connections
        = new();

    public StockHub(ILogger<StockHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var user = Context.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            // Check for admin using either role or full api access scope
            var isAdmin = user.IsInRole(AuthConstants.SystemAdministratorRole) ||
                          user.HasClaim(c => c.Type == "scope" && c.Value == AuthConstants.FullApiAccess);

            if (isAdmin)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "SystemAdministrators");
                _logger.LogInformation("Admin connected to StockHub");
            }

            // Check for inventory access
            var hasInventoryAccess = isAdmin ||
                                     user.IsInRole(AuthConstants.InventoryManagerRole) ||
                                     user.HasClaim(c => c.Type == "Permission" && c.Value == "Inventory.Manage");

            if (hasInventoryAccess)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Inventory");
                _logger.LogInformation("Inventory manager connected to StockHub");
            }
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_connections.TryRemove(Context.ConnectionId, out var connection))
        {
            // Remove from groups
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "SystemAdministrators");
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Inventory");

            // Notify admins of disconnection
            if (connection.Roles.Contains("SystemAdministrator"))
            {
                await Clients.Group("SystemAdministrators").UserDisconnected(new
                {
                    connection.Username,
                    DisconnectedAt = DateTime.UtcNow
                });
            }

            _logger.LogInformation(
                "User {Username} disconnected with connection ID {ConnectionId}",
                connection.Username,
                connection.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendStockUpdate(Guid productId, int newStock)
    {
        await Clients.All.SendStockUpdate(productId, newStock);
    }

    public async Task SendLowStockAlert(Guid productId, int currentStock, int threshold)
    {
        await Clients.All.SendLowStockAlert(new
        {
            ProductId = productId,
            CurrentStock = currentStock,
            LowStockThreshold = threshold,
            IsLowStock = true
        });
    }

    public async Task SubscribeToProduct(string productId)
    {
        var groupName = $"product-{productId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        _logger.LogInformation(
            "User {ConnectionId} subscribed to product {ProductId}",
            Context.ConnectionId,
            productId);
    }

    public async Task UnsubscribeFromProduct(string productId)
    {
        var groupName = $"product-{productId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

        _logger.LogInformation(
            "User {ConnectionId} unsubscribed from product {ProductId}",
            Context.ConnectionId,
            productId);
    }

    public async Task JoinInventoryMonitoring()
    {
        if (Context.User?.IsInRole("SystemAdministrator") == true)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "InventoryMonitors");

            _logger.LogInformation(
                "User {ConnectionId} joined inventory monitoring",
                Context.ConnectionId);
        }
        else
        {
            throw new HubException("Unauthorized to monitor inventory");
        }
    }

    public async Task SendInventoryNotification(string message, string level = "info")
    {
        if (Context.User?.IsInRole(AuthConstants.InventoryManagerRole) == true ||
            Context.User?.IsInRole(AuthConstants.SystemAdministratorRole) == true)
        {
            await Clients.Group("InventoryMonitors").SendInventoryNotification(new
            {
                Message = message,
                Level = level,
                Timestamp = DateTime.UtcNow,
                Sender = Context.User.Identity!.Name ?? "Unknown User"
            });

            _logger.LogInformation(
                "Inventory notification sent by {UserName}: {Level} - {Message}",
                Context.User.Identity!.Name ?? "Unknown User",
                level,
                message);
        }
        else
        {
            throw new HubException("Unauthorized to send inventory notifications");
        }
    }
}