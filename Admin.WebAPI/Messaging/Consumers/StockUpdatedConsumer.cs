using Admin.Application.Products.Events;
using Admin.WebAPI.Hubs;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace Admin.WebAPI.Messaging.Consumers;

public class StockUpdatedConsumer : IConsumer<ProductStockUpdatedIntegrationEvent>
{
    private readonly IHubContext<StockHub> _hubContext;
    private readonly ILogger<StockUpdatedConsumer> _logger;

    public StockUpdatedConsumer(
        IHubContext<StockHub> hubContext,
        ILogger<StockUpdatedConsumer> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProductStockUpdatedIntegrationEvent> context)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("StockUpdated", new
            {
                context.Message.ProductId,
                context.Message.NewStock
            });

            _logger.LogInformation("Successfully notified clients about stock update for product {ProductId}",
                context.Message.ProductId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing StockUpdatedEvent for product {ProductId}",
                context.Message.ProductId);
            throw;
        }
    }
}