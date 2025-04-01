// Admin.Application/Orders/DTOs/OrderDto.cs
using Admin.Domain.Enums;

using System;
using System.Collections.Generic;

namespace Admin.Application.Orders.DTOs;
public record OrderDto
{
    public Guid Id { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public Guid CustomerId { get; init; }
    public string Status { get; init; } = string.Empty; // String representation of OrderStatus enum
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
    public string PaymentStatus { get; init; } = string.Empty;
    public string PaymentMethod { get; init; } = string.Empty;
    public ShippingInfoDto? ShippingInfo { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? CreatedBy { get; init; }
    public DateTime? LastModifiedAt { get; init; }
    public string? LastModifiedBy { get; init; }
}
