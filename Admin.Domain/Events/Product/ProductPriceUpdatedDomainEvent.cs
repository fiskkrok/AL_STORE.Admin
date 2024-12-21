using Admin.Domain.Common;
using Admin.Domain.Entities;
using Admin.Domain.ValueObjects;

namespace Admin.Domain.Events.Product;
public record ProductPriceUpdatedDomainEvent(Entities.Product Product, Money OldPrice, Money NewPrice) : DomainEvent;

