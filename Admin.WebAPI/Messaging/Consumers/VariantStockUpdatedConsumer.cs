using Admin.Application.Products.Events;
using Admin.WebAPI.Hubs;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace Admin.WebAPI.Messaging.Consumers
{
    public class VariantStockUpdatedConsumer : IConsumer<ProductVariantStockUpdatedIntegrationEvent>
    {
        private readonly IHubContext<StockHub> _hubContext;
        private readonly ILogger<VariantStockUpdatedConsumer> _logger;

        public VariantStockUpdatedConsumer(
            IHubContext<StockHub> hubContext,
            ILogger<VariantStockUpdatedConsumer> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ProductVariantStockUpdatedIntegrationEvent> context)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("VariantStockUpdated", new
                {
                    context.Message.ProductId,
                    context.Message.VariantId,
                    context.Message.NewStock
                });

                _logger.LogInformation(
                    "Successfully notified clients about stock update for product {ProductId}, variant {VariantId}",
                    context.Message.ProductId,
                    context.Message.VariantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error processing VariantStockUpdatedEvent for product {ProductId}, variant {VariantId}",
                    context.Message.ProductId,
                    context.Message.VariantId);
                throw;
            }
        }
    }
}
