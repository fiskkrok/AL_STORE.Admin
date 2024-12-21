using Admin.Domain.Common;
using Admin.Domain.Entities;

namespace Admin.Domain.Events.Product;

public record ProductDeletedDomainEvent(Entities.Product Product) : DomainEvent;