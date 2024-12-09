using Admin.Application.Common.Interfaces;

namespace Admin.Application.Products.Events;
// Integration Events
public class ProductVariantCreatedIntegrationEvent : IMessage
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    public Guid ProductId { get; init; }
    public Guid VariantId { get; init; }
    public string Sku { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Currency { get; init; } = string.Empty;
    public int Stock { get; init; }
}
