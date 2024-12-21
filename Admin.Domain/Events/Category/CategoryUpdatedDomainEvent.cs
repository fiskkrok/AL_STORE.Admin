using Admin.Domain.Common;
using Admin.Domain.Entities;

namespace Admin.Domain.Events.Category;

public record CategoryUpdatedDomainEvent(Entities.Category Category) : DomainEvent;