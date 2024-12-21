using System.Collections.Concurrent;
using System.Security.Claims;

using Admin.WebAPI.Hubs.Interface;
using Admin.WebAPI.Hubs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Admin.WebAPI.Hubs;
//ProductHub.cs - Updated implementation
[Authorize]
public class ProductHub : Hub<IProductHubClient>
{
    private readonly ILogger<ProductHub> _logger;
    private readonly IConfiguration _configuration;
    private static readonly ConcurrentDictionary<string, UserConnection> _connections
        = new();

    public ProductHub(
        ILogger<ProductHub> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public override async Task OnConnectedAsync()
    {
        var user = Context.User;
        if (user?.Identity?.Name != null)
        {
            var connection = new UserConnection
            {
                UserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                Username = user.Identity.Name,
                ConnectionId = Context.ConnectionId,
                ConnectedAt = DateTime.UtcNow,
                Roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
            };

            _connections.TryAdd(Context.ConnectionId, connection);

            // Add to appropriate groups based on roles
            if (user.IsInRole("Admin"))
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");

            if (user.IsInRole("Inventory"))
                await Groups.AddToGroupAsync(Context.ConnectionId, "Inventory");

            // Notify admins of new connection
            if (connection.Roles.Contains("Admin"))
            {
                await Clients.Group("Admins").UserConnected(new
                {
                    connection.Username,
                    connection.ConnectedAt
                });
            }

            _logger.LogInformation(
                "User {Username} connected with connection ID {ConnectionId}",
                connection.Username,
                connection.ConnectionId);
        }

        await base.OnConnectedAsync();
    }

   
        public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_connections.TryRemove(Context.ConnectionId, out var connection))
        {
            // Remove from groups
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Admins");
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Inventory");

            // Notify admins of disconnection
            if (connection.Roles.Contains("Admin"))
            {
                await Clients.Group("Admins").UserDisconnected(new
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
        if (Context.User?.IsInRole("Inventory") == true)
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

    public async Task SendInventoryAlert(string message)
    {
        if (Context.User?.IsInRole("Inventory") == true)
        {
            await Clients.Group("InventoryMonitors").InventoryAlert(new
            {
                Message = message,
                Timestamp = DateTime.UtcNow,
                Sender = Context.User.Identity!.Name
            });

            _logger.LogInformation(
                "Inventory alert sent by {User}: {Message}",
                Context.User.Identity!.Name,
                message);
        }
        else
        {
            throw new HubException("Unauthorized to send inventory alerts");
        }
    }
}