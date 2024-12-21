using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using Admin.Application.Orders.Events;
using Admin.Domain.Events.Order;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Admin.Application.EventHandler;

public class OrderEventHandler :
    INotificationHandler<DomainEventNotification<OrderCreatedDomainEvent>>,
    INotificationHandler<DomainEventNotification<OrderStatusChangedDomainEvent>>,
    INotificationHandler<DomainEventNotification<OrderPaymentAddedDomainEvent>>,
    INotificationHandler<DomainEventNotification<OrderShippingInfoSetDomainEvent>>,
    INotificationHandler<DomainEventNotification<OrderCancelledDomainEvent>>
{
    private readonly IMessageBusService _messageBus;
    private readonly ILogger<OrderEventHandler> _logger;

    public OrderEventHandler(
        IMessageBusService messageBus,
        ILogger<OrderEventHandler> logger)
    {
        _messageBus = messageBus;
        _logger = logger;
    }

    public async Task Handle(
        DomainEventNotification<OrderCreatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var order = notification.DomainEvent.Order;

        try
        {
            var integrationEvent = new OrderCreatedIntegrationEvent
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerId = order.CustomerId,
                Total = order.Total.Amount,
                Currency = order.Total.Currency,
                Status = order.Status
            };

            await _messageBus.PublishAsync(integrationEvent, cancellationToken);

            _logger.LogInformation(
                "Published OrderCreatedIntegrationEvent for order {OrderNumber}",
                order.OrderNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error publishing OrderCreatedIntegrationEvent for order {OrderNumber}",
                order.OrderNumber);
            throw;
        }
    }

    public async Task Handle(
        DomainEventNotification<OrderStatusChangedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        try
        {
            var integrationEvent = new OrderStatusChangedIntegrationEvent
            {
                OrderId = domainEvent.Order.Id,
                OrderNumber = domainEvent.Order.OrderNumber,
                OldStatus = domainEvent.OldStatus,
                NewStatus = domainEvent.NewStatus,
                StatusChangedAt = DateTime.UtcNow
            };

            await _messageBus.PublishAsync(integrationEvent, cancellationToken);

            _logger.LogInformation(
                "Published OrderStatusChangedIntegrationEvent for order {OrderNumber}: {OldStatus} -> {NewStatus}",
                domainEvent.Order.OrderNumber,
                domainEvent.OldStatus,
                domainEvent.NewStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error publishing OrderStatusChangedIntegrationEvent for order {OrderNumber}",
                domainEvent.Order.OrderNumber);
            throw;
        }
    }

    public async Task Handle(
        DomainEventNotification<OrderPaymentAddedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var order = notification.DomainEvent.Order;

        try
        {
            if (order.Payment == null) return;

            var integrationEvent = new OrderPaymentAddedIntegrationEvent
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                TransactionId = order.Payment.TransactionId,
                PaymentMethod = order.Payment.Method,
                Amount = order.Payment.Amount.Amount,
                Currency = order.Payment.Amount.Currency,
                Status = order.Payment.Status,
                ProcessedAt = order.Payment.ProcessedAt
            };

            await _messageBus.PublishAsync(integrationEvent, cancellationToken);

            _logger.LogInformation(
                "Published OrderPaymentAddedIntegrationEvent for order {OrderNumber}",
                order.OrderNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error publishing OrderPaymentAddedIntegrationEvent for order {OrderNumber}",
                order.OrderNumber);
            throw;
        }
    }

    public async Task Handle(
        DomainEventNotification<OrderShippingInfoSetDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var order = notification.DomainEvent.Order;

        try
        {
            if (order.ShippingInfo == null) return;

            var integrationEvent = new OrderShippingUpdatedIntegrationEvent
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                Carrier = order.ShippingInfo.Carrier,
                TrackingNumber = order.ShippingInfo.TrackingNumber,
                EstimatedDeliveryDate = order.ShippingInfo.EstimatedDeliveryDate
            };

            await _messageBus.PublishAsync(integrationEvent, cancellationToken);

            _logger.LogInformation(
                "Published OrderShippingUpdatedIntegrationEvent for order {OrderNumber}",
                order.OrderNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error publishing OrderShippingUpdatedIntegrationEvent for order {OrderNumber}",
                order.OrderNumber);
            throw;
        }
    }

    public async Task Handle(
        DomainEventNotification<OrderCancelledDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var order = notification.DomainEvent.Order;

        try
        {
            var integrationEvent = new OrderCancelledIntegrationEvent
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                CancellationReason = order.CancellationReason ?? string.Empty,
                CancelledAt = order.CancelledAt ?? DateTime.UtcNow
            };

            await _messageBus.PublishAsync(integrationEvent, cancellationToken);

            _logger.LogInformation(
                "Published OrderCancelledIntegrationEvent for order {OrderNumber}",
                order.OrderNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error publishing OrderCancelledIntegrationEvent for order {OrderNumber}",
                order.OrderNumber);
            throw;
        }
    }
}