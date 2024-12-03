using Admin.Application.Common.Interfaces;

namespace Admin.Application.Products.Events;
public class ImageProcessedIntegrationEvent : IMessage
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    public Guid ProductId { get; init; }
    public Guid ImageId { get; init; }
    public string ProcessedUrl { get; init; } = string.Empty;
}
