using Admin.Application.Common.Interfaces;

namespace Admin.Application.Products.Events;
public class ProductStockUpdatedIntegrationEvent : IMessage
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    public Guid ProductId { get; init; }
    public int NewStock { get; init; }
}
