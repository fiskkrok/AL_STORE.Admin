using Admin.Application.Orders.Events;
using Admin.WebAPI.Hubs;
using Admin.WebAPI.Hubs.Interface;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace Admin.WebAPI.Messaging.Consumers;

public class OrderPaymentAddedConsumer : IConsumer<OrderPaymentAddedIntegrationEvent>
{
    private readonly IHubContext<OrderHub, IOrderHubClient> _hubContext;
    private readonly ILogger<OrderPaymentAddedConsumer> _logger;

    public OrderPaymentAddedConsumer(
        IHubContext<OrderHub, IOrderHubClient> hubContext,
        ILogger<OrderPaymentAddedConsumer> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderPaymentAddedIntegrationEvent> context)
    {
        try
        {
            // Notify relevant clients about the payment
            await _hubContext.Clients.Group($"customer-{context.Message.OrderId}")
                .OrderPaymentProcessed(
                    context.Message.OrderId,
                    context.Message.OrderNumber,
                    context.Message.TransactionId,
                    context.Message.Status);

            _logger.LogInformation(
                "Successfully notified clients about payment for order {OrderNumber}",
                context.Message.OrderNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing OrderPaymentAddedIntegrationEvent for order {OrderNumber}",
                context.Message.OrderNumber);
            throw;
        }
    }
}
