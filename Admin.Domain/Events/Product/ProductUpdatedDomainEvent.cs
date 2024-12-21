using Admin.Domain.Common;
using Admin.Domain.Entities;

namespace Admin.Domain.Events.Product;

public record ProductUpdatedDomainEvent(Entities.Product Product) : DomainEvent;