using Admin.Domain.Common;
using Admin.Domain.Entities;

namespace Admin.Domain.Events;
// ProductVariantDeletedDomainEvent.cs
public record ProductVariantDeletedDomainEvent(Product Product, ProductVariant Variant) : DomainEvent;
