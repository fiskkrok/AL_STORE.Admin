using Admin.Domain.Common;

namespace Admin.Domain.Events.Product;

public record ProductDeletedDomainEvent(Entities.Product Product) : DomainEvent;