using Admin.Domain.Common;
using Admin.Domain.Entities;

namespace Admin.Domain.Events.Product;

public record ProductImageAddedDomainEvent(Entities.Product Product, ProductImage Image) : DomainEvent;