using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin.Application.Orders.DTOs.External;
public class ExternalOrderDto
{
    public string Source { get; init; } = string.Empty;
    public string ExternalId { get; init; } = string.Empty;
    public string ExternalOrderNumber { get; init; } = string.Empty;
    public string Currency { get; init; } = string.Empty;
    public Guid CustomerId { get; init; }
    public AddressDto ShippingAddress { get; init; } = null!;
    public AddressDto BillingAddress { get; init; } = null!;
    public List<ExternalOrderItemDto> Items { get; init; } = new();
    public decimal ShippingCost { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal DiscountAmount { get; init; }
    public string? CouponCode { get; init; }
    public string? Notes { get; init; }
    public DateTime OrderDate { get; init; }
    public string? RequestId { get; init; } // For idempotency
    public string? Signature { get; init; } // For verification
}
