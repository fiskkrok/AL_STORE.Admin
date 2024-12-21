using Admin.Domain.Common;
using Admin.Domain.Entities;

namespace Admin.Domain.Events.ProductVariant;
// ProductVariantCreatedDomainEvent.cs
public record ProductVariantCreatedDomainEvent(Entities.Product Product, Entities.ProductVariant Variant) : DomainEvent;
