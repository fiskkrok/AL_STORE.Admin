using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin.Application.Orders.DTOs.External;
public class ExternalOrderItemDto
{
    public string ExternalProductId { get; init; } = string.Empty;
    public Guid ProductId { get; init; }
    public Guid? VariantId { get; init; }
    public string Sku { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal DiscountAmount { get; init; }
}