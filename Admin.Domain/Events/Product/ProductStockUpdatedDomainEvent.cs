using Admin.Domain.Common;

namespace Admin.Domain.Events.Product;

public record ProductStockUpdatedDomainEvent(Entities.Product Product, int OldStock, int NewStock) : DomainEvent;