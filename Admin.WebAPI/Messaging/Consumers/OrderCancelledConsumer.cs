using Admin.Application.Orders.Events;
using Admin.WebAPI.Hubs;
using Admin.WebAPI.Hubs.Interface;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace Admin.WebAPI.Messaging.Consumers;

public class OrderCancelledConsumer : IConsumer<OrderCancelledIntegrationEvent>
{
    private readonly IHubContext<OrderHub, IOrderHubClient> _hubContext;
    private readonly ILogger<OrderCancelledConsumer> _logger;

    public OrderCancelledConsumer(
        IHubContext<OrderHub, IOrderHubClient> hubContext,
        ILogger<OrderCancelledConsumer> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCancelledIntegrationEvent> context)
    {
        try
        {
            // Notify relevant clients about the cancellation
            await _hubContext.Clients.Group($"customer-{context.Message.OrderId}")
                .OrderCancelled(
                    context.Message.OrderId,
                    context.Message.OrderNumber,
                    context.Message.CancellationReason,
                    context.Message.CancelledAt);

            // Also notify admin group about cancellation
            await _hubContext.Clients.Group("admin")
                .OrderCancelled(
                    context.Message.OrderId,
                    context.Message.OrderNumber,
                    context.Message.CancellationReason,
                    context.Message.CancelledAt);

            _logger.LogInformation(
                "Successfully notified clients about cancellation of order {OrderNumber}",
                context.Message.OrderNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing OrderCancelledIntegrationEvent for order {OrderNumber}",
                context.Message.OrderNumber);
            throw;
        }
    }
}
