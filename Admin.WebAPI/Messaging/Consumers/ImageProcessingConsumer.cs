using Admin.Domain.Events.Product;
using Admin.WebAPI.Hubs;
using Admin.WebAPI.Hubs.Interface;

using MassTransit;

using Microsoft.AspNetCore.SignalR;

namespace Admin.WebAPI.Messaging.Consumers;

public class ImageProcessingConsumer : IConsumer<ProductImageProcessingEvent>
{
    private readonly IHubContext<ProductHub, IProductHubClient> _hubContext;
    private readonly ILogger<ImageProcessingConsumer> _logger;

    public ImageProcessingConsumer(
        IHubContext<ProductHub, IProductHubClient> hubContext,
        ILogger<ImageProcessingConsumer> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProductImageProcessingEvent> context)
    {
        try
        {
            await _hubContext.Clients.All.ImageProcessed(
                context.Message.ProductId,
                context.Message.ImageId,
                context.Message.ProcessedUrl);

            _logger.LogInformation("Successfully notified clients about processed image for product {ProductId}",
                context.Message.ProductId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing ImageProcessingEvent for product {ProductId}",
                context.Message.ProductId);
            throw;
        }
    }
}