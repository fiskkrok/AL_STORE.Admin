using Admin.Domain.Common;

namespace Admin.Domain.Events.Order;

public record OrderShippingInfoSetDomainEvent(Entities.Order Order) : DomainEvent;