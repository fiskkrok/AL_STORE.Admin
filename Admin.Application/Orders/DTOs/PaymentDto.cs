using Admin.Domain.Enums;

namespace Admin.Application.Orders.DTOs;
public record PaymentDto
{
    public string TransactionId { get; init; } = string.Empty;
    public PaymentMethod Method { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public PaymentStatus Status { get; init; }
    public DateTime ProcessedAt { get; init; }
}
