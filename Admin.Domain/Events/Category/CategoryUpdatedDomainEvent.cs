using Admin.Domain.Common;

namespace Admin.Domain.Events.Category;

public record CategoryUpdatedDomainEvent(Entities.Category Category) : DomainEvent;