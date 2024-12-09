using Admin.Domain.Common;
using Admin.Domain.Entities;

namespace Admin.Domain.Events;
// ProductVariantStockUpdatedDomainEvent.cs
public record ProductVariantStockUpdatedDomainEvent(
    Product Product,
    ProductVariant Variant,
    int OldStock,
    int NewStock) : DomainEvent;
