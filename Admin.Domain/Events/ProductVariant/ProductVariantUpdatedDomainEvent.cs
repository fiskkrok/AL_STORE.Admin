using Admin.Domain.Common;
using Admin.Domain.Entities;

namespace Admin.Domain.Events.ProductVariant;
// ProductVariantUpdatedDomainEvent.cs
public record ProductVariantUpdatedDomainEvent(Entities.Product Product, Entities.ProductVariant Variant) : DomainEvent;
