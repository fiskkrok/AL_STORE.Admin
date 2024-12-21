using Admin.Domain.Common;
using Admin.Domain.Entities;

namespace Admin.Domain.Events.ProductVariant;
// ProductVariantDeletedDomainEvent.cs
public record ProductVariantDeletedDomainEvent(Entities.Product Product, Entities.ProductVariant Variant) : DomainEvent;
