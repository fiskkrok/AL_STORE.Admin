using Admin.Domain.Common;
using Admin.Domain.Entities;

namespace Admin.Domain.Events.Category;

public record CategoryCreatedDomainEvent(Entities.Category Category) : DomainEvent;