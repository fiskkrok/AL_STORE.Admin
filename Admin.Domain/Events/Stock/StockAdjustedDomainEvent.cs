using Admin.Domain.Common;
using Admin.Domain.Entities;

namespace Admin.Domain.Events.Stock;

public record StockAdjustedDomainEvent(StockItem StockItem, int OldStock, int NewStock, string Reason) : DomainEvent;