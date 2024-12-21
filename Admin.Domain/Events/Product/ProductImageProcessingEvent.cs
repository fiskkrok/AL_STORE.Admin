using Admin.Domain.Common;

namespace Admin.Domain.Events.Product;

public record ProductImageProcessingEvent(Guid ProductId,
    Guid ImageId,
    string ProcessedUrl) : DomainEvent;
