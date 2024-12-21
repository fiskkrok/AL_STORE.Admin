using Admin.Domain.Common;

namespace Admin.Domain.Events.ProductVariant;
// ProductVariantUpdatedDomainEvent.cs
public record ProductVariantUpdatedDomainEvent(Entities.Product Product, Entities.ProductVariant Variant) : DomainEvent;
