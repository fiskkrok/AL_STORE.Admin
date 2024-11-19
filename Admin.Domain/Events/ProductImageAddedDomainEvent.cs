using Admin.Domain.Common;
using Admin.Domain.Entities;

namespace Admin.Domain.Events;

public record ProductImageAddedDomainEvent(Product Product, ProductImage Image) : DomainEvent;