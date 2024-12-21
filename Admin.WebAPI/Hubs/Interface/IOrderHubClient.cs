using Admin.Domain.Enums;

namespace Admin.WebAPI.Hubs.Interface;

// Define the OrderHub interface that clients will implement
public interface IOrderHubClient
{
    Task OrderStatusChanged(Guid orderId, string orderNumber, OrderStatus oldStatus, OrderStatus newStatus);
    Task OrderPaymentProcessed(Guid orderId, string orderNumber, string transactionId, PaymentStatus status);
    Task OrderShippingUpdated(Guid orderId, string orderNumber, string carrier, string trackingNumber, DateTime estimatedDeliveryDate);
    Task OrderCancelled(Guid orderId, string orderNumber, string reason, DateTime cancelledAt);
}
