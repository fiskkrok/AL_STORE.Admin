using Admin.Domain.Common;

namespace Admin.Domain.Events.Category;

public record CategoryCreatedDomainEvent(Entities.Category Category) : DomainEvent;