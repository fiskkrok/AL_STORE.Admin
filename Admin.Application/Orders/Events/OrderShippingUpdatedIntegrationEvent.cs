using Admin.Application.Common.Interfaces;

namespace Admin.Application.Orders.Events;

public class OrderShippingUpdatedIntegrationEvent : IMessage
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    public Guid OrderId { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public string Carrier { get; init; } = string.Empty;
    public string TrackingNumber { get; init; } = string.Empty;
    public DateTime EstimatedDeliveryDate { get; init; }
}