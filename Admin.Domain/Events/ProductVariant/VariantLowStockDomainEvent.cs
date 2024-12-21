using Admin.Domain.Common;

namespace Admin.Domain.Events.ProductVariant;
public record VariantLowStockDomainEvent(
    Entities.ProductVariant Variant) : DomainEvent;
