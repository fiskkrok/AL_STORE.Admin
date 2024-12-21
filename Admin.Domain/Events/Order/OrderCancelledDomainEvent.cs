using Admin.Domain.Common;

namespace Admin.Domain.Events.Order;

public record OrderCancelledDomainEvent(Entities.Order Order) : DomainEvent;