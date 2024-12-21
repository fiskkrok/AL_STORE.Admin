using Admin.Domain.Common;
using Admin.Domain.Entities;

namespace Admin.Domain.Events.Product;

public record ProductStockUpdatedDomainEvent(Entities.Product Product, int OldStock, int NewStock) : DomainEvent;