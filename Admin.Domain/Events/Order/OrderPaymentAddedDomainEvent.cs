using Admin.Domain.Common;

namespace Admin.Domain.Events.Order;

public record OrderPaymentAddedDomainEvent(Entities.Order Order) : DomainEvent;