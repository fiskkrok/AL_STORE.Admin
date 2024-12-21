using Admin.Domain.Common;
using Admin.Domain.Entities;

namespace Admin.Domain.Events.Product;

public record ProductCreatedDomainEvent(Entities.Product Product) : DomainEvent;