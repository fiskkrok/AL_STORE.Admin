using Admin.WebAPI.Hubs.Interface;
using Admin.WebAPI.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Admin.WebAPI.Hubs;

[Authorize(AuthenticationSchemes = $"{AuthConstants.JwtBearerScheme},{AuthConstants.ApiKeyScheme}")]
public class OrderHub : Hub<IOrderHubClient>
{
    private readonly ILogger<OrderHub> _logger;

    public OrderHub(ILogger<OrderHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var customerId = Context.User?.FindFirst("sub")?.Value;
        if (!string.IsNullOrEmpty(customerId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"customer-{customerId}");
            _logger.LogInformation("Customer {CustomerId} connected to OrderHub", customerId);
        }

        if (Context.User?.IsInRole("Admin") == true)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "admin");
            _logger.LogInformation("Admin user connected to OrderHub");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var customerId = Context.User?.FindFirst("sub")?.Value;
        if (!string.IsNullOrEmpty(customerId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"customer-{customerId}");
            _logger.LogInformation("Customer {CustomerId} disconnected from OrderHub", customerId);
        }

        if (Context.User?.IsInRole("Admin") == true)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "admin");
            _logger.LogInformation("Admin user disconnected from OrderHub");
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SubscribeToOrder(string orderNumber)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"order-{orderNumber}");
        _logger.LogInformation(
            "Client {ConnectionId} subscribed to order {OrderNumber}",
            Context.ConnectionId,
            orderNumber);
    }

    public async Task UnsubscribeFromOrder(string orderNumber)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order-{orderNumber}");
        _logger.LogInformation(
            "Client {ConnectionId} unsubscribed from order {OrderNumber}",
            Context.ConnectionId,
            orderNumber);
    }
}
