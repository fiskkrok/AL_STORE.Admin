using Admin.Application.Common.Interfaces;

namespace Admin.Application.Products.Events;
public class ProductVariantStockUpdatedIntegrationEvent : IMessage
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    public Guid ProductId { get; init; }
    public Guid VariantId { get; init; }
    public int NewStock { get; init; }
}
