using Admin.Domain.Common;

namespace Admin.Domain.Events.ProductVariant;
// ProductVariantCreatedDomainEvent.cs
public record ProductVariantCreatedDomainEvent(Entities.Product Product, Entities.ProductVariant Variant) : DomainEvent;
