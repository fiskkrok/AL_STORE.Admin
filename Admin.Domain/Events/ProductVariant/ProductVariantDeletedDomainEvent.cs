using Admin.Domain.Common;

namespace Admin.Domain.Events.ProductVariant;
// ProductVariantDeletedDomainEvent.cs
public record ProductVariantDeletedDomainEvent(Entities.Product Product, Entities.ProductVariant Variant) : DomainEvent;
