using Admin.Domain.Enums;

namespace Admin.Application.Orders.DTOs;
public record PaymentDto
{
    public string TransactionId { get; init; } = string.Empty;
    public string Method { get; init; } = string.Empty; // String representation of PaymentMethod enum
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty; // String representation of PaymentStatus enum
    public DateTime ProcessedAt { get; init; }
}
