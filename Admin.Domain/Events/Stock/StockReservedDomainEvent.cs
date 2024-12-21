using Admin.Domain.Common;
using Admin.Domain.Entities;

namespace Admin.Domain.Events.Stock;

public record StockReservedDomainEvent(StockItem StockItem, Guid OrderId, int Quantity) : DomainEvent;