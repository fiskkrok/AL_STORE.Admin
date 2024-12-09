using Admin.Domain.Common;
using Admin.Domain.Entities;

namespace Admin.Domain.Events;
// ProductVariantUpdatedDomainEvent.cs
public record ProductVariantUpdatedDomainEvent(Product Product, ProductVariant Variant) : DomainEvent;
