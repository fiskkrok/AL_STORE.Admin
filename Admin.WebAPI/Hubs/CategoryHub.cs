using Admin.WebAPI.Hubs.Interface;
using Microsoft.AspNetCore.SignalR;

namespace Admin.WebAPI.Hubs;

public class CategoryHub : Hub<ICategoryHubClient>
{
    private readonly ILogger<CategoryHub> _logger;

    public CategoryHub(ILogger<CategoryHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var customerId = Context.User?.FindFirst("sub")?.Value;
        if (!string.IsNullOrEmpty(customerId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"customer-{customerId}");
            _logger.LogInformation("Customer {CustomerId} connected to CategoryHub", customerId);
        }

        if (Context.User?.IsInRole("Admin") == true)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "admin");
            _logger.LogInformation("Admin user connected to CategoryHub");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var customerId = Context.User?.FindFirst("sub")?.Value;
        if (!string.IsNullOrEmpty(customerId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"customer-{customerId}");
            _logger.LogInformation("Customer {CustomerId} disconnected from CategoryHub", customerId);
        }

        if (Context.User?.IsInRole("Admin") == true)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "admin");
            _logger.LogInformation("Admin user disconnected from CategoryHub");
        }

        await base.OnDisconnectedAsync(exception);
    }
}
