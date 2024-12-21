using Admin.Application.Orders.Events;
using Admin.WebAPI.Hubs;
using Admin.WebAPI.Hubs.Interface;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace Admin.WebAPI.Messaging.Consumers;

public class OrderStatusChangedConsumer : IConsumer<OrderStatusChangedIntegrationEvent>
{
    private readonly IHubContext<OrderHub, IOrderHubClient> _hubContext;
    private readonly ILogger<OrderStatusChangedConsumer> _logger;

    public OrderStatusChangedConsumer(
        IHubContext<OrderHub, IOrderHubClient> hubContext,
        ILogger<OrderStatusChangedConsumer> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderStatusChangedIntegrationEvent> context)
    {
        try
        {
            // Notify all clients about the status change
            await _hubContext.Clients.All.OrderStatusChanged(
                context.Message.OrderId,
                context.Message.OrderNumber,
                context.Message.OldStatus,
                context.Message.NewStatus);

            // Notify specific customer about their order
            await _hubContext.Clients.Group($"customer-{context.Message.OrderId}")
                .OrderStatusChanged(
                    context.Message.OrderId,
                    context.Message.OrderNumber,
                    context.Message.OldStatus,
                    context.Message.NewStatus);

            _logger.LogInformation(
                "Successfully notified clients about status change for order {OrderNumber}",
                context.Message.OrderNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing OrderStatusChangedIntegrationEvent for order {OrderNumber}",
                context.Message.OrderNumber);
            throw;
        }
    }
}
