using Admin.Domain.Common;
using Admin.Domain.Entities;

namespace Admin.Domain.Events.Category;

public record CategoryDeletedDomainEvent(Entities.Category Category) : DomainEvent;