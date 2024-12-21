using Admin.Domain.Common;

namespace Admin.Domain.Events.Category;

public record CategoryDeletedDomainEvent(Entities.Category Category) : DomainEvent;