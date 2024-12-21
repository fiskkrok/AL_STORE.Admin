using Admin.Domain.Common;
using Admin.Domain.Entities;

namespace Admin.Domain.Events.Stock;

public record StockReservationCancelledDomainEvent(StockItem StockItem, Guid OrderId, int Quantity) : DomainEvent;