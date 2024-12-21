using Admin.Domain.Common;

namespace Admin.Domain.Events.ProductVariant;
// ProductVariantStockUpdatedDomainEvent.cs
public record ProductVariantStockUpdatedDomainEvent(
    Entities.Product Product,
    Entities.ProductVariant Variant,
    int OldStock,
    int NewStock) : DomainEvent;
