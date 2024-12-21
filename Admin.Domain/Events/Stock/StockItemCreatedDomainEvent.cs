using Admin.Domain.Common;
using Admin.Domain.Entities;

namespace Admin.Domain.Events.Stock;
// Domain Events
public record StockItemCreatedDomainEvent(StockItem StockItem) : DomainEvent;