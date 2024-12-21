using Admin.Domain.Common;

namespace Admin.Domain.Events.Product;

public record ProductCreatedDomainEvent(Entities.Product Product) : DomainEvent;