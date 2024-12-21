using Admin.Domain.Common;
using Admin.Domain.Entities;

namespace Admin.Domain.Events.Stock;

public record LowStockDomainEvent(StockItem StockItem) : DomainEvent;