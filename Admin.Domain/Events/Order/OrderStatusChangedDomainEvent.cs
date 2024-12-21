using Admin.Domain.Common;
using Admin.Domain.Enums;

namespace Admin.Domain.Events.Order;

public record OrderStatusChangedDomainEvent(Entities.Order Order, OrderStatus OldStatus, OrderStatus NewStatus) : DomainEvent;