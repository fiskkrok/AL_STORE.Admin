using Admin.Domain.Common;

namespace Admin.Domain.Events.Product;

public record ProductUpdatedDomainEvent(Entities.Product Product) : DomainEvent;