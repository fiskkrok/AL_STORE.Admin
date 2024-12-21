using Admin.Domain.Common;

namespace Admin.Domain.Events.Order;

public record OrderShippingAddressUpdatedDomainEvent(Entities.Order Order) : DomainEvent;