using Admin.Application.Common.Interfaces;
using Admin.Domain.Enums;

namespace Admin.Application.Orders.Events;

public class OrderStatusChangedIntegrationEvent : IMessage
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    public Guid OrderId { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public OrderStatus OldStatus { get; init; }
    public OrderStatus NewStatus { get; init; }
    public DateTime StatusChangedAt { get; init; }
}