using Admin.Domain.Common;
using Admin.Domain.Entities;

namespace Admin.Domain.Events;

public record ProductStockUpdatedDomainEvent(Product Product, int OldStock, int NewStock) : DomainEvent;