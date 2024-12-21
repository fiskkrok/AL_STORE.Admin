using Admin.Application.Orders.Events;
using Admin.WebAPI.Hubs;
using Admin.WebAPI.Hubs.Interface;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace Admin.WebAPI.Messaging.Consumers;

public class OrderShippingUpdatedConsumer : IConsumer<OrderShippingUpdatedIntegrationEvent>
{
    private readonly IHubContext<OrderHub, IOrderHubClient> _hubContext;
    private readonly ILogger<OrderShippingUpdatedConsumer> _logger;

    public OrderShippingUpdatedConsumer(
        IHubContext<OrderHub, IOrderHubClient> hubContext,
        ILogger<OrderShippingUpdatedConsumer> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderShippingUpdatedIntegrationEvent> context)
    {
        try
        {
            // Notify relevant clients about the shipping update
            await _hubContext.Clients.Group($"customer-{context.Message.OrderId}")
                .OrderShippingUpdated(
                    context.Message.OrderId,
                    context.Message.OrderNumber,
                    context.Message.Carrier,
                    context.Message.TrackingNumber,
                    context.Message.EstimatedDeliveryDate);

            _logger.LogInformation(
                "Successfully notified clients about shipping update for order {OrderNumber}",
                context.Message.OrderNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing OrderShippingUpdatedIntegrationEvent for order {OrderNumber}",
                context.Message.OrderNumber);
            throw;
        }
    }
}
