using Admin.Application.Orders.DTOs;
using Admin.Domain.Enums;

namespace Admin.WebAPI.Endpoints.Orders.Responses;

public record OrderResponse
{
    public OrderResponse(OrderDto order)
    {
        Id = order.Id;
        OrderNumber = order.OrderNumber;
        CustomerId = order.CustomerId;
        Status = order.Status;
        Subtotal = order.Subtotal;
        ShippingCost = order.ShippingCost;
        Tax = order.Tax;
        Total = order.Total;
        Currency = order.Currency;
        ShippingAddress = new AddressResponse(order.ShippingAddress);
        BillingAddress = new AddressResponse(order.BillingAddress);
        Notes = order.Notes;
        CancelledAt = order.CancelledAt;
        CancellationReason = order.CancellationReason;
        Items = order.Items.Select(i => new OrderItemResponse(i)).ToList();
        Payment = order.Payment != null ? new PaymentResponse(order.Payment) : null;
        ShippingInfo = order.ShippingInfo != null ? new ShippingInfoResponse(order.ShippingInfo) : null;
        CreatedAt = order.CreatedAt;
        CreatedBy = order.CreatedBy;
        LastModifiedAt = order.LastModifiedAt;
        LastModifiedBy = order.LastModifiedBy;
    }

    public Guid Id { get; init; }
    public string OrderNumber { get; init; }
    public Guid CustomerId { get; init; }
    public OrderStatus Status { get; init; }
    public decimal Subtotal { get; init; }
    public decimal ShippingCost { get; init; }
    public decimal Tax { get; init; }
    public decimal Total { get; init; }
    public string Currency { get; init; }
    public AddressResponse ShippingAddress { get; init; }
    public AddressResponse BillingAddress { get; init; }
    public string? Notes { get; init; }
    public DateTime? CancelledAt { get; init; }
    public string? CancellationReason { get; init; }
    public List<OrderItemResponse> Items { get; init; }
    public PaymentResponse? Payment { get; init; }
    public ShippingInfoResponse? ShippingInfo { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? CreatedBy { get; init; }
    public DateTime? LastModifiedAt { get; init; }
    public string? LastModifiedBy { get; init; }
}