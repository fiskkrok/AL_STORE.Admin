using Admin.Domain.Common;
using Admin.Domain.ValueObjects;

namespace Admin.Domain.Events.ProductVariant;
public record VariantPriceUpdatedDomainEvent(
    Entities.ProductVariant Variant,
    Money OldPrice,
    Money NewPrice) : DomainEvent;
