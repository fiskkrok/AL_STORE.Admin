using Admin.WebAPI.Hubs.Interface;

using Microsoft.AspNetCore.SignalR;

namespace Admin.WebAPI.Hubs;

public class ProductVariantHub : Hub<IVariantHubClient>
{
    private readonly ILogger<ProductVariantHub> _logger;

    public ProductVariantHub(ILogger<ProductVariantHub> logger)
    {
        _logger = logger;
    }

    public async Task SubscribeToVariant(string variantId)
    {
        var groupName = $"variant-{variantId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        _logger.LogInformation(
            "User {ConnectionId} subscribed to variant {VariantId}",
            Context.ConnectionId,
            variantId);
    }

    public async Task UnsubscribeFromVariant(string variantId)
    {
        var groupName = $"variant-{variantId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

        _logger.LogInformation(
            "User {ConnectionId} unsubscribed from variant {VariantId}",
            Context.ConnectionId,
            variantId);
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
}
