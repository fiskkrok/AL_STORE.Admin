using Admin.Domain.Enums;

namespace Admin.Application.Orders.DTOs;
public record OrderDto
{
    public Guid Id { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public Guid CustomerId { get; init; }
    public OrderStatus Status { get; init; }
    public decimal Subtotal { get; init; }
    public decimal ShippingCost { get; init; }
    public decimal Tax { get; init; }
    public decimal Total { get; init; }
    public string Currency { get; init; } = string.Empty;
    public AddressDto ShippingAddress { get; init; } = null!;
    public AddressDto BillingAddress { get; init; } = null!;
    public string? Notes { get; init; }
    public DateTime? CancelledAt { get; init; }
    public string? CancellationReason { get; init; }
    public List<OrderItemDto> Items { get; init; } = new();
    public PaymentDto? Payment { get; init; }
    public ShippingInfoDto? ShippingInfo { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? CreatedBy { get; init; }
    public DateTime? LastModifiedAt { get; init; }
    public string? LastModifiedBy { get; init; }
}
