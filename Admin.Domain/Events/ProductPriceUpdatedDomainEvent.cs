using Admin.Domain.Common;
using Admin.Domain.Entities;
using Admin.Domain.ValueObjects;

namespace Admin.Domain.Events;
public record ProductPriceUpdatedDomainEvent(Product Product, Money OldPrice, Money NewPrice) : DomainEvent;

