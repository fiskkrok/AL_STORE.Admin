using Admin.Domain.Common;

namespace Admin.Domain.Events.Order;

public record OrderItemAddedDomainEvent(Entities.Order Order, Guid ProductId, int Quantity) : DomainEvent;