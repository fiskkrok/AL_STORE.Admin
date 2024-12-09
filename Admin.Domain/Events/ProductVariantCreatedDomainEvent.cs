using Admin.Domain.Common;
using Admin.Domain.Entities;

namespace Admin.Domain.Events;
// ProductVariantCreatedDomainEvent.cs
public record ProductVariantCreatedDomainEvent(Product Product, ProductVariant Variant) : DomainEvent;
