using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Admin.Domain.Common;
using Admin.Domain.Entities;
using Admin.Domain.ValueObjects;

namespace Admin.Domain.Events.ProductVariant;
public record VariantPriceUpdatedDomainEvent(
    Entities.ProductVariant Variant,
    Money OldPrice,
    Money NewPrice) : DomainEvent;
