using Admin.Domain.Common;

namespace Admin.Domain.Events.Order;
public record OrderCreatedDomainEvent(Entities.Order Order) : DomainEvent;