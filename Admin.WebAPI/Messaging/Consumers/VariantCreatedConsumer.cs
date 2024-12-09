using Admin.Application.Products.Events;
using Admin.Application.ProductVariants.Queries;
using Admin.WebAPI.Hubs;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.SignalR;


namespace Admin.WebAPI.Messaging.Consumers;

public class VariantCreatedConsumer : IConsumer<ProductVariantCreatedIntegrationEvent>
{
    private readonly IHubContext<ProductHub, IProductHubClient> _hubContext;
    private readonly IMediator _mediator;
    private readonly ILogger<VariantCreatedConsumer> _logger;

    public VariantCreatedConsumer(
        IHubContext<ProductHub, IProductHubClient> hubContext,
        IMediator mediator,
        ILogger<VariantCreatedConsumer> logger)
    {
        _hubContext = hubContext;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProductVariantCreatedIntegrationEvent> context)
    {
        try
        {
            var query = new GetProductVariantQuery(context.Message.ProductId, context.Message.VariantId);
            var result = await _mediator.Send(query);

            if (result.IsSuccess && result.Value != null)
            {
                await _hubContext.Clients.All.VariantCreated(
                    context.Message.ProductId,
                    result.Value);

                _logger.LogInformation(
                    "Successfully notified clients about new variant: {VariantId} for product {ProductId}",
                    context.Message.VariantId,
                    context.Message.ProductId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing VariantCreatedIntegrationEvent for product {ProductId}, variant {VariantId}",
                context.Message.ProductId,
                context.Message.VariantId);
            throw;
        }
    }
}