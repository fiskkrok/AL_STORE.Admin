using Admin.Application.Products.Events;
using Admin.Application.Products.Queries;
using Admin.WebAPI.Hubs;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace Admin.WebAPI.Messaging.Consumers;

public class ProductCreatedConsumer : IConsumer<ProductCreatedIntegrationEvent>
{
    private readonly IHubContext<ProductHub, IProductHubClient> _hubContext;
    private readonly ILogger<ProductCreatedConsumer> _logger;
    private readonly IMediator _mediator;

    public ProductCreatedConsumer(
        IHubContext<ProductHub, IProductHubClient> hubContext,
        IMediator mediator,
        ILogger<ProductCreatedConsumer> logger)
    {
        _hubContext = hubContext;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProductCreatedIntegrationEvent> context)
    {
        try
        {
            var query = new GetProductQuery(context.Message.ProductId);
            var result = await _mediator.Send(query);

            if (result.IsSuccess && result.Value != null)
            {
                await _hubContext.Clients.All.ProductCreated(result.Value);
                _logger.LogInformation("Successfully notified clients about new product: {ProductId}",
                    context.Message.ProductId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing ProductCreatedIntegrationEvent for product {ProductId}",
                context.Message.ProductId);
            throw;
        }
    }
}