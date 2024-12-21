using Admin.Domain.Common;

namespace Admin.Domain.Events.Order;

public record OrderItemRemovedDomainEvent(Entities.Order Order, Guid ProductId) : DomainEvent;