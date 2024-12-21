using Admin.Application.Common.Interfaces;

namespace Admin.Application.Orders.Events;

public class OrderCancelledIntegrationEvent : IMessage
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    public Guid OrderId { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public string CancellationReason { get; init; } = string.Empty;
    public DateTime CancelledAt { get; init; }
}