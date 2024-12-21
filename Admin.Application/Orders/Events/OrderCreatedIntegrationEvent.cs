using Admin.Application.Common.Interfaces;
using Admin.Domain.Enums;

namespace Admin.Application.Orders.Events;
public class OrderCreatedIntegrationEvent : IMessage
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    public Guid OrderId { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public Guid CustomerId { get; init; }
    public decimal Total { get; init; }
    public string Currency { get; init; } = string.Empty;
    public OrderStatus Status { get; init; }
}