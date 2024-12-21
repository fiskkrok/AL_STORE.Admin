using Admin.Application.Common.Interfaces;
using Admin.Domain.Enums;

namespace Admin.Application.Orders.Events;

public class OrderPaymentAddedIntegrationEvent : IMessage
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    public Guid OrderId { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public string TransactionId { get; init; } = string.Empty;
    public PaymentMethod PaymentMethod { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public PaymentStatus Status { get; init; }
    public DateTime ProcessedAt { get; init; }
}